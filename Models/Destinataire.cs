using System;
using System.Collections.Generic;

namespace RobotUniversign.Models
{
    public class Destinataire
    {
        public string Email { get; set; }
        public string Prenom { get; set; }
        public string Nom { get; set; }
        public string Mobile { get; set; }// Optionel
        public string Certificat { get; set; }
        public InformationSignature Signature { get; set; }
    }
}
