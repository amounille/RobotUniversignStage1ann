using Commun_DocuWare_pck;
using DocuWare.Platform.ServerClient;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RobotUniversign.DAO;
using RobotUniversign.Models;
using RobotUniversign.Services;
using System;
using System.IO;
using System.Net;
using Universign_pck.Models;

namespace RobotUniversign.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BatchController : ControllerBase
    {
        
        private DatabaseContext _bdd;
        public BatchController(IDBService Bdd)
        {
            _bdd = Bdd.GetDatabase();

           
        }

        [HttpPost]
        [Produces("application/json")]
        public Retour Post([FromBody] InputData Data)
        {
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

                _bdd.Batch.Add(new DAO.Entities.Batch()
                {
                    Armoire = Data.Armoire,
                    Document = Data.Document
                });

                _bdd.SaveChanges();
                
                return Retour.RetourOK();
            } catch (Exception ex)
            {
               
                var errMessage = ex.Message;

                if (!Equals(ex.InnerException, null))
                {
                    errMessage = errMessage + " - " + ex.InnerException.Message;
                }

                return Retour.RetourERREUR(errMessage);
            }
        }
    }
}