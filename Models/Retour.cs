namespace RobotUniversign.Models {
    public class Retour {
        public string Statut { get; set; }
        public string Message { get; set; }

        public static Retour RetourOK() {
            return new Retour() {
                Statut = "OK"
            };
        }
        public static Retour RetourERREUR(string Message) {
            return new Retour() {
                Statut = "ERREUR",
                Message = Message
            };
        }
    }
}