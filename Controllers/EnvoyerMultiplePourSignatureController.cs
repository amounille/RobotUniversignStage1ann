using Commun_DocuWare_pck;
using DocuWare.Platform.ServerClient;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RobotUniversign.DAO;
using RobotUniversign.Models;
using RobotUniversign.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Universign_pck.Models;

namespace RobotUniversign.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnvoyerMultiplePourSignatureController : ControllerBase 
    {
        private ConfDW _cdw;
        private readonly ILogger<EnvoyerMultiplePourSignatureController> _logger;
        private DatabaseContext _bdd;

        public EnvoyerMultiplePourSignatureController(ILogger<EnvoyerMultiplePourSignatureController> logger, IDBService Bdd)
        {
            _logger = logger;

            _bdd = Bdd.GetDatabase();

            _cdw = new ConfDW()
            {
                Langue = "fr",
                Orga = "Vienne Documentique",
                Url = "https://viennedocument.docuware.cloud/DocuWare/Platform",
                Login = "Theo",
                Token = "b8dd9188-b6ad-41cc-8fc6-1668f208d9af_13d39830-9055-4c95-97a7-6b75b4adba15"
            };
        }

        [HttpPost]
        [Produces("application/json")]
        public Retour Post([FromBody] InputDataMultipleSignature Data)
        {
            VDocuWare docuware = null; // Interface DocuWare
            try
            {
                // CONTROLE DES PARAMETRES

                if (string.IsNullOrEmpty(Data.CodeClient))
                {
                    throw new ArgumentException("Le paramètre 'CodeClient' ne peut pas être null");
                }

                if (string.IsNullOrEmpty(Data.Armoire))
                {
                    throw new ArgumentException("Le paramètre 'Armoire' ne peut pas être null");
                }

                if (Equals(Data.Document, 0))
                {
                    throw new ArgumentException("Le paramètre 'Document' ne peut pas être null");
                }

                //Debut de la partie check de destinataire

                if (Equals(Data.Destinataire, null))
                {
                    throw new ArgumentException("Le paramètre 'Destinataire' ne peut pas être null");
                }
                foreach (var item in Data.Destinataire) { 

                 if (string.IsNullOrEmpty(item.Email))
                 {
                     throw new ArgumentException("Le paramètre 'Email' ne peut pas être null");
                 }


                 if (string.IsNullOrEmpty(item.Prenom))
                 {
                     throw new ArgumentException("Le paramètre 'Prenom' ne peut pas être null");
                 }

                 if (string.IsNullOrEmpty(item.Nom))
                 {
                     throw new ArgumentException("Le paramètre 'Nom' ne peut pas être null");
                 }

                 if (string.IsNullOrEmpty(item.Certificat))
                 {
                     throw new ArgumentException("Le paramètre 'Certificat' ne peut pas être null");
                 }

                    //Debut de la partie check de Signature

                if (Equals(item.Signature, null))
                {
                    throw new ArgumentException("Le paramètre 'Signature' ne peut pas être null");
                    }
                    if (Equals(item.Signature.Page, 0))
                    {
                        throw new ArgumentException("Le paramètre 'Page' ne peut pas être null");
                    }
                    if (Equals(item.Signature.X, 0))
                    {
                        throw new ArgumentException("Le paramètre 'X' ne peut pas être null");
                    }
                    if (Equals(item.Signature.Y, 0))
                    {
                        throw new ArgumentException("Le paramètre 'Y' ne peut pas être null");
                    }

                    //fin de la partie check de Signature
                }

                if (Equals(Data.EnvoyerMail, null))
                {
                    throw new ArgumentException("Le paramètre 'EnvoyerMail' ne peut pas être null");
                }

                if (Equals(Data.EnvoyerDocument, null))
                {
                    throw new ArgumentException("Le paramètre 'EnvoyerDocument' ne peut pas être null");
                }

                if (string.IsNullOrEmpty(Data.Langue))
                {
                    throw new ArgumentException("Le paramètre 'Langue' ne peut pas être null");
                }



                //fin de la partie check de destinataire


                // TDDO: CHECK Value

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

                var fileCabinet = docuware.GetFileCabinet(Data.Armoire, false);

                if (Equals(fileCabinet, null))
                {
                    throw new Exception("Armoire introuvable");
                }

                var document = docuware.GetDocument(fileCabinet, Data.Document);

                if (Equals(document, null))
                {
                    throw new Exception("Document introuvable");
                }

                var tempPath = Path.Combine(Path.GetTempPath(), "RobotUS", Data.CodeClient, Data.Document.ToString());
                if (!Directory.Exists(tempPath)) {
                    Directory.CreateDirectory(tempPath);
                }

                var filePath = docuware.Download(document, tempPath, FileDownloadType.PDF);

                var us = new Universign_pck.Universign("fthibaut@viennedoc.com", "4HA9JZRN", false);

                var trans = new TransactionRequest
                {
                    profile = "default",
                    signers = new TransactionSigner[Data.Destinataire.Count],
                    documents = new TransactionDocument[1],
                    certificateTypes = new string[Data.Destinataire.Count],
                    mustContactFirstSigner = Data.EnvoyerMail,
                    finalDocSent = Data.EnvoyerDocument,
                    //identificationType = "none", // Pas dans la doc mais dans demo ?
                    language = Data.Langue
                };

                trans.documents[0] = new TransactionDocument()
                {
                    content = System.IO.File.ReadAllBytes(filePath),
                    name = Path.GetFileName(filePath)
                };

                for (int i = 0; i < Data.Destinataire.Count; i++)
                {
                    var signataire = Data.Destinataire[i];


                    trans.signers[i] = new TransactionSigner()
                    {
                        firstname = signataire.Prenom,
                        lastname = signataire.Nom,
                        emailAddress = signataire.Email,
                        signatureField = new Universign_pck.Models.SignatureField()
                        {
                            page = signataire.Signature.Page,
                            x = signataire.Signature.X,
                            y = signataire.Signature.Y,
                        }
                    };

                    //


                    trans.certificateTypes[i] = signataire.Certificat;

                }

                var result = us.Signature(trans);

                _bdd.Collecte.Add(new DAO.Entities.Collect()
                {
                    Transaction = result.id,
                    Armoire = fileCabinet.Id,
                    Document = Data.Document
                });

                _bdd.SaveChanges();

                docuware.Close();
                return Retour.RetourOK();
            } catch (Exception ex)
            {
                try {
                    docuware.Close(); 
                } catch {
                   // ignored
                }

                return Retour.RetourERREUR(ex.Message);
            }
        }
    }
}

