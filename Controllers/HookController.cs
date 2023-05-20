using Commun_DocuWare_pck;
using DocuWare.Platform.ServerClient;
using Hangfire;
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
    public class HookController : ControllerBase 
    {
        private DatabaseContext _bdd;

        public HookController(IDBService Bdd)
        {
            _bdd = Bdd.GetDatabase();
            
            
        }

        [HttpGet]
        [Produces("application/json")]
        public Retour Get([FromQuery] int Status, [FromQuery] string Id)
        {
            BackgroundJob.Enqueue(() => new HookService(_bdd).TraiterCallback(Status, Id));
                return Retour.RetourOK();
        }


    }
}