using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Mardis.Engine.DataAccess.MardisCore
{
    [Table("ColdEquipment_Tospy", Schema = "MardisCore")]
    public class ColdEquipment_Tospy
    {
        [Key]
        public int IDEquipment { get; set; }
        public string ID { get; set; }

        public Guid IDtask { get; set; }

        [ForeignKey("IDtask")]
        public TaskCampaign TaskCampaigns { get; set; }

        public string CODIGO { get; set; }

        public string TIPO_OK { get; set; }

        public string CANASTILLA_OK { get; set; }


        public string PIES_OK { get; set; }
        public string BRANDEO_OK { get; set; }

        public string MARCA_OK { get; set; }


        public string PLACA { get; set; }

        public string SERIE { get; set; }
        public string STICKER { get; set; }

        public string FUNCIONA { get; set; }


        public string ENCENDIDO { get; set; }
        public string PUERTA_OK { get; set; }
        public string VINIVL_OK { get; set; }

        public string CENEFA_OK { get; set; }


        public string CONTAMINADO { get; set; }
        public string STICKER_TIENE { get; set; }


        public int secuencial { get; set; }


    }
}
