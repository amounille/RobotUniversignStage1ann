using Commun_DocuWare_pck;
using DocuWare.Platform.ServerClient;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RobotUniversign.DAO;
using RobotUniversign.Models;
using System;
using System.IO;
using System.Linq;
using System.Net;


namespace RobotUniversign.Services
{
    public class HookService

    {
        private ConfDW _cdw;
        private DatabaseContext _bdd;
        private readonly ILogger<HookService> _logger;



        public HookService(DatabaseContext Bdd)
        {
            _bdd = Bdd;

            _cdw = new ConfDW()
            {
                Langue = "fr",
                Orga = "Vienne Documentique",
                Url = "https://viennedocument.docuware.cloud/DocuWare/Platform",
                Login = "Theo",
                Token = "b8dd9188-b6ad-41cc-8fc6-1668f208d9af_13d39830-9055-4c95-97a7-6b75b4adba15"
            };
        }

        // No retry
        public void TraiterCallback(int Status, string TransactionId)
        {
            var collecte = _bdd.Collecte.First(c => c.Transaction == TransactionId);
            
            if (Equals(Status, 1) || Equals(Status, 3) || Equals(Status, 4))
            {
                BackgroundJob.Enqueue(() => new HookService(_bdd).CallbackErreur(collecte.Id));
            }

            if (Equals(Status, 2))
            {
                BackgroundJob.Enqueue(() => new HookService(_bdd).MAJDocument(collecte.Id));
            }

            /*if (Equals(Status, 5))
            {

            }*/

            /*0 : prêt (en attente du prochain signataire),
            o 1 : expiré(collecte créée mais non terminée après 14 jours), 
            o 2 : session complétée terminée (tous les signataires ont signé), 
            o 3 : annulation de la session par un signataire,
            o 4 : échec(technique) de la session de signature,
            o 5 : en attente de validation par l’autorité d’inscription d’Universign (les signataires ont 
            signé mais les pièces d’identité sont en cours de vérification afin d’établir une identité 
            numérique)*/
        }
       
        public void MAJDocument(int BddId)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "RobotUS", "Hook", BddId.ToString());
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            // On va chercher la  collecte dans la BDD
            var collecte = _bdd.Collecte.First(c => c.Id == BddId);

            //https://monsite.fr/infos?signer=numstatut(0à5)&status=0&id=357bbb40-8a9d-31e8-b15f-bbee6243ba65

            // on se connecte a universign et on récupére le document signé gràce à l'identifiant de collecte

            // ON faitt un annule et remplace dans DW : 

            VDocuWare docuware = null; // Interface DocuWare
            FileCabinet fileCabinet; // Contenant l'armoire que l'on passe en paramètre
            DocuWare.Platform.ServerClient.Document documentOrig; // Contenant le document original





            try
            {
           
                // Test URL
                try
                {
                    WebRequest req = WebRequest.Create(_cdw.Url);
                    WebResponse res = req.GetResponse();
                }
                catch
                {
                    throw new Exception("URL " + _cdw.Url + " incorrecte ou non accessible");
                }

                // Connexion DocuWare
                try
                {
                    
                    docuware = new VDocuWare(_cdw.Url, _cdw.Token, _cdw.Orga, _cdw.Langue);
                }
                catch
                {
                    throw new Exception("Connexion Docuware impossible, veuillez vérifer que le token est correct dans la configuration");
                }

                

                fileCabinet = docuware.GetFileCabinetById(collecte.Armoire); // On a besoin de l'armoire ou est le document

                if (Equals(fileCabinet, null))
                {
                    throw new Exception("Armoire introuvable");
                }

                documentOrig = docuware.GetDocument(fileCabinet, collecte.Document); // On récupère le document original

                
   
                var us = new Universign_pck.Universign("fthibaut@viennedoc.com", "4HA9JZRN", false);

                var docUs = us.GetDocument(collecte.Transaction);



                var filePath = Path.Combine(tempPath, docUs.name + ".pdf");

                File.WriteAllBytes(filePath, docUs.content);

                foreach (var section in documentOrig.Sections.Skip(1))
                {
                    section.DeleteSelfRelation();
                }


                docuware.ReplaceFile(documentOrig.Sections.First().GetSectionFromSelfRelation(), filePath);

                docuware.Close();

                System.IO.File.Delete(filePath);
                Directory.Delete(tempPath);

                // TODO: Supprimer ligne dans BDD.
            }
            catch (Exception ex)
            {


                try
                {
                    docuware.Close();
                }
                catch
                {
                    // ignored
                }

                throw ex;

            }
        }
        // on supprime la collecte dans la BDD : 

        // Bdd = _bdd.Collecte.

        

        public void CallbackErreur(int BddId)
        {
            // ON va charcher la collecte dans la BDD

            // EXEC BDD.Collect; (test)
            var collecte = _bdd.Collecte.First(c => c.Id == BddId);
            // ON la supprime

            _bdd.Collecte.Remove(collecte);
            _bdd.SaveChanges();

        }

    }
}