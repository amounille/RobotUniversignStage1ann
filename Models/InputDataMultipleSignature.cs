using System.Collections.Generic;

namespace RobotUniversign.Models
{
    public class InputDataMultipleSignature : InputData
    {
        public List<Destinataire> Destinataire { get; set; }
        public bool EnvoyerMail { get; set; }
        public bool EnvoyerDocument { get; set; }
        public string Langue { get; set; }

    }
}