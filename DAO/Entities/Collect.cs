using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RobotUniversign.DAO.Entities
{
    [Table("collect")]
    public class Collect
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Transaction { get; set; }

        public string Armoire { get; set; }

        public int Document { get; set; }
    }
}
