using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RobotUniversign.DAO.Entities
{ 
    [Table("batch")]
    public class Batch
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Armoire { get; set; }

        public int Document { get; set; }
    }
}
