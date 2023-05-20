using Commun_DocuWare_pck;
using DocuWare.Platform.ServerClient;
using Hangfire;
using Hangfire.MissionControl;
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
    [MissionLauncher(CategoryName = "Cron")]
    public class BatchService

    {
        private ConfDW _cdw;
        private DatabaseContext _bdd;



        public BatchService(DatabaseContext Bdd)
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

        [Mission(Name = "Configurer Cron")]
        [AutomaticRetry(Attempts = 0)]
        public void SetupCron()
        {
            RecurringJob.AddOrUpdate("TamponnerUS", () => new BatchService(_bdd).Tamponer(), Cron.Daily(23));
        }

    [Mission(Name = "Test")]
        public void Tamponer()
        {
        var aTamponer = _bdd.Batch.OrderBy(b => b.Armoire).ToList();

        if (aTamponer.Count != 0)
        {
                var now = DateTime.Now;

                var tempPath = Path.Combine(Path.GetTempPath(), "RobotUS", "BATCH", now.Day + "_" + now.Month + "_" + now.Year + "_" + now.Hour + "_" + now.Minute + "_" + now.Second);
                if (!Directory.Exists(tempPath))
                {
                    Directory.CreateDirectory(tempPath);
                }

                var us = new Universign_pck.Universign("fthibaut@viennedoc.com", "4HA9JZRN", false);

                VDocuWare dw = null;

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
                    dw = new VDocuWare(_cdw.Url, _cdw.Token, _cdw.Orga, _cdw.Langue);
                }
                catch
                {
                    throw new Exception("Connexion Docuware impossible, veuillez vérifer que le token est correct dans la configuration");
                }

                var armoireActuelle = aTamponer.First().Armoire;
                var fc = dw.GetFileCabinet(armoireActuelle);


            foreach (var monDoc in aTamponer) {
                if (!Equals(armoireActuelle, monDoc.Armoire)) {
                        armoireActuelle = monDoc.Armoire;
                        fc = dw.GetFileCabinet(armoireActuelle);
                }

                var doc = dw.GetDocument(fc, monDoc.Document);

                    if (Equals(doc, null))
                    {
                        throw new Exception("Document introuvable");
                    }


                    var filePath = dw.Download(doc, tempPath, FileDownloadType.PDF);
                    var data = us.CachetServeur(filePath);

                    File.WriteAllBytes(filePath, data);


                    // Annule et remplace


                    File.Delete(filePath);

                    _bdd.Remove(monDoc);
                    _bdd.SaveChanges();
                }

                Directory.Delete(tempPath, true);

                dw.Close();
            }
        }

    }
}