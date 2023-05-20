namespace RobotUniversign.Models
{
    public class InputDataSignature : InputData
    {
        public Destinataire Destinataire { get; set; } // objet ListeDestinataire  
        public bool EnvoyerMail { get; set; }
        public bool EnvoyerDocument { get; set; }
        public string Langue { get; set; }

    }
}