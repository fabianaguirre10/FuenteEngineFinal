CREATE TABLE [MardisCore].[coldEquipment_Tospy]
(
[IDtask] [uniqueidentifier] NULL,
[ID] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CODIGO] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[TIPO_OK] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CANASTILLA_OK] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[PIES_OK] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[BRANDEO_OK] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[MARCA_OK] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[PLACA] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[SERIE] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[STICKER] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[FUNCIONA] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[ENCENDIDO] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[PUERTA_OK] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[VINIVL_OK] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CENEFA_OK] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[CONTAMINADO] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[STICKER_TIENE] [varchar] (max) COLLATE SQL_Latin1_General_CP1_CI_AS NULL,
[secuencial] [int] NULL CONSTRAINT [DF__coldEquip__secue__3E8A7B91] DEFAULT ((0)),
[IDEquipment] [int] NOT NULL IDENTITY(1, 1)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [MardisCore].[coldEquipment_Tospy] ADD CONSTRAINT [PK__coldEqui__728B117765524F6E] PRIMARY KEY CLUSTERED ([IDEquipment])
GO
CREATE NONCLUSTERED INDEX [idx_task_index] ON [MardisCore].[coldEquipment_Tospy] ([IDtask])
GO
ALTER TABLE [MardisCore].[coldEquipment_Tospy] ADD CONSTRAINT [FK__coldEquip__IDtas__2B629884] FOREIGN KEY ([IDtask]) REFERENCES [MardisCore].[Task] ([Id])
GO
