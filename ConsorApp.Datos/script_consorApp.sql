/* =====================================================================
   ConsorApp - Script de instalacion de la base de datos
   Motor: SQL Server (instancia .\SQLEXPRESS)   Base: consorAppDb

   COMO USARLO
     1. Abrir SQL Server Management Studio y conectarse a .\SQLEXPRESS.
     2. Abrir este archivo y presionar Ejecutar (F5) UNA vez, completo.
     3. Iniciar la aplicacion con alguno de los usuarios de prueba.

   El script es re-ejecutable: no duplica tablas ni datos iniciales.

   USUARIOS DE PRUEBA (contrasenia de los tres: 1234)
     admin        -> perfil Administrador
     encargado    -> perfil Encargado
     propietario  -> perfil Propietario
   Los datos personales de estos usuarios son de relleno (DNI 0000000X,
   telefono 0000000000). Cambiarlos desde Gestion Usuarios.
   ===================================================================== */

USE [master]
GO

IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = N'consorAppDb')
BEGIN
    CREATE DATABASE [consorAppDb]
END
GO

USE [consorAppDb]
GO

SET NOCOUNT ON
GO

/* ---------------------------------------------------------------------
   1. SEGURIDAD: Perfiles y Usuarios
   --------------------------------------------------------------------- */

IF OBJECT_ID(N'dbo.Perfiles', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Perfiles (
        IdPerfil      INT IDENTITY(1,1) NOT NULL,
        NombrePerfil  VARCHAR(50)       NOT NULL,
        CONSTRAINT PK_Perfiles PRIMARY KEY CLUSTERED (IdPerfil)
    )
END
GO

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        IdUsuario       INT IDENTITY(1,1) NOT NULL,
        Nombre          VARCHAR(50)       NOT NULL,
        Apellido        VARCHAR(50)       NOT NULL,
        Dni             VARCHAR(20)       NOT NULL,
        Telefono        VARCHAR(30)       NULL,
        Email           VARCHAR(100)      NOT NULL,
        UsuarioSistema  VARCHAR(50)       NOT NULL,
        Contrasenia     VARCHAR(100)      NOT NULL,
        IdPerfil        INT               NOT NULL,
        Estado          INT               NOT NULL CONSTRAINT DF_Usuarios_Estado DEFAULT 1,
        FechaAlta       DATETIME          NOT NULL CONSTRAINT DF_Usuarios_FechaAlta DEFAULT GETDATE(),
        CONSTRAINT PK_Usuarios PRIMARY KEY CLUSTERED (IdUsuario),
        CONSTRAINT UQ_Usuarios_Dni UNIQUE (Dni),
        CONSTRAINT UQ_Usuarios_UsuarioSistema UNIQUE (UsuarioSistema),
        CONSTRAINT FK_Usuarios_Perfiles FOREIGN KEY (IdPerfil) REFERENCES dbo.Perfiles (IdPerfil)
    )
END
GO

/* ---------------------------------------------------------------------
   2. EDIFICIO, DEPARTAMENTOS Y PROPIETARIOS
   --------------------------------------------------------------------- */

IF OBJECT_ID(N'dbo.Edificio', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Edificio (
        id_edificio  INT IDENTITY(1,1) NOT NULL,
        descripcion  VARCHAR(150)      NULL,
        CantDepto    INT               NOT NULL,
        cantPisos    INT               NOT NULL,
        ubicacion    VARCHAR(150)      NOT NULL,
        CONSTRAINT PK_Edificio PRIMARY KEY CLUSTERED (id_edificio)
    )
END
GO

IF OBJECT_ID(N'dbo.Departamento', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Departamento (
        id_Departamento  INT IDENTITY(1,1) NOT NULL,
        id_Edificio      INT               NOT NULL,
        piso             INT               NOT NULL,
        unidad           VARCHAR(10)       NOT NULL,
        CONSTRAINT PK_Departamento PRIMARY KEY CLUSTERED (id_Departamento),
        CONSTRAINT FK_Departamento_Edificio FOREIGN KEY (id_Edificio) REFERENCES dbo.Edificio (id_edificio)
    )
END
GO

/* Un departamento puede tener varios propietarios a lo largo del tiempo;
   el vigente es el que tiene estado = 1 y fechaHasta NULL. */
IF OBJECT_ID(N'dbo.Propietario', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Propietario (
        id_Propietario   INT IDENTITY(1,1) NOT NULL,
        Id_Usuario       INT               NULL,
        id_Departamento  INT               NULL,
        fechaDesde       DATE              NOT NULL,
        fechaHasta       DATE              NULL,
        estado           VARCHAR(20)       NOT NULL,
        CONSTRAINT PK_Propietario PRIMARY KEY CLUSTERED (id_Propietario),
        CONSTRAINT FK_Propietario_Usuario FOREIGN KEY (Id_Usuario) REFERENCES dbo.Usuarios (IdUsuario),
        CONSTRAINT FK_Propietario_Departamento FOREIGN KEY (id_Departamento) REFERENCES dbo.Departamento (id_Departamento)
    )
END
GO

/* ---------------------------------------------------------------------
   3. ESPACIOS COMUNES, TURNOS, RESERVAS, BLOQUEOS E INCIDENTES
   --------------------------------------------------------------------- */

IF OBJECT_ID(N'dbo.espacioComun', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.espacioComun (
        id_espacioComun  INT IDENTITY(1,1) NOT NULL,
        id_edificio      INT               NOT NULL,
        nombre           VARCHAR(100)      NOT NULL,
        descripcion      VARCHAR(250)      NULL,
        capacidad        INT               NOT NULL,
        estado           VARCHAR(20)       NOT NULL,
        CONSTRAINT PK_espacioComun PRIMARY KEY CLUSTERED (id_espacioComun),
        CONSTRAINT FK_espacioComun_Edificio FOREIGN KEY (id_edificio) REFERENCES dbo.Edificio (id_edificio)
    )
END
GO

IF OBJECT_ID(N'dbo.Turno', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Turno (
        id_Turno    INT IDENTITY(1,1) NOT NULL,
        nombreTurno VARCHAR(20)       NOT NULL,
        horaInicio  TIME              NOT NULL,
        horaFin     TIME              NOT NULL,
        CONSTRAINT PK_Turno PRIMARY KEY CLUSTERED (id_Turno)
    )
END
GO

IF OBJECT_ID(N'dbo.Reserva', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reserva (
        id_Reserva         INT IDENTITY(1,1) NOT NULL,
        id_EspacioComun    INT               NOT NULL,
        id_Propietario     INT               NOT NULL,
        id_Turno           INT               NOT NULL,
        codigoSeguimiento  VARCHAR(50)       NOT NULL,
        fechaReserva       DATE              NOT NULL,
        estadoReserva      VARCHAR(20)       NOT NULL,
        motivoCancelacion  VARCHAR(200)      NULL,
        CONSTRAINT PK_Reserva PRIMARY KEY CLUSTERED (id_Reserva),
        CONSTRAINT FK_Reserva_espacioComun FOREIGN KEY (id_EspacioComun) REFERENCES dbo.espacioComun (id_espacioComun),
        CONSTRAINT FK_Reserva_Propietario FOREIGN KEY (id_Propietario) REFERENCES dbo.Propietario (id_Propietario),
        CONSTRAINT FK_Reserva_Turno FOREIGN KEY (id_Turno) REFERENCES dbo.Turno (id_Turno)
    )
END
GO

IF OBJECT_ID(N'dbo.BloqueoEspacio', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.BloqueoEspacio (
        id_Bloqueo       INT IDENTITY(1,1) NOT NULL,
        id_espacioComun  INT               NOT NULL,
        fechaDesde       DATE              NOT NULL,
        fechaHasta       DATE              NOT NULL,
        motivo           VARCHAR(200)      NOT NULL,
        CONSTRAINT PK_BloqueoEspacio PRIMARY KEY CLUSTERED (id_Bloqueo),
        CONSTRAINT FK_Bloqueo_EspacioComun FOREIGN KEY (id_espacioComun) REFERENCES dbo.espacioComun (id_espacioComun)
    )
END
GO

IF OBJECT_ID(N'dbo.Incidente', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Incidente (
        Id_Incidente     INT IDENTITY(1,1) NOT NULL,
        Id_Reserva       INT               NOT NULL,
        Descripcion      VARCHAR(500)      NOT NULL,
        FechaIncidente   DATETIME          NOT NULL CONSTRAINT DF_Incidente_Fecha DEFAULT GETDATE(),
        EstadoIncidente  VARCHAR(30)       NOT NULL,
        CONSTRAINT PK_Incidente PRIMARY KEY CLUSTERED (Id_Incidente),
        CONSTRAINT FK_Incidente_Reserva FOREIGN KEY (Id_Reserva) REFERENCES dbo.Reserva (id_Reserva)
    )
END
GO

/* ---------------------------------------------------------------------
   4. AVISOS Y RECLAMOS
   --------------------------------------------------------------------- */

/* id_edificio y id_Departamento vacios = aviso general; con
   id_Departamento = aviso dirigido a una unidad. estado 1 = activo. */
IF OBJECT_ID(N'dbo.Aviso', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Aviso (
        id_Aviso          INT IDENTITY(1,1) NOT NULL,
        id_usuario        INT               NOT NULL,
        id_edificio       INT               NULL,
        id_Departamento   INT               NULL,
        titulo            VARCHAR(100)      NOT NULL,
        mensaje           VARCHAR(MAX)      NOT NULL,
        fechaPublicacion  DATETIME          NOT NULL,
        estado            INT               NOT NULL CONSTRAINT DF_Aviso_Estado DEFAULT 1,
        CONSTRAINT PK_Aviso PRIMARY KEY CLUSTERED (id_Aviso),
        CONSTRAINT FK_Aviso_Usuarios FOREIGN KEY (id_usuario) REFERENCES dbo.Usuarios (IdUsuario),
        CONSTRAINT FK_Aviso_Edificio FOREIGN KEY (id_edificio) REFERENCES dbo.Edificio (id_edificio) ON DELETE SET NULL,
        CONSTRAINT FK_Aviso_Departamento FOREIGN KEY (id_Departamento) REFERENCES dbo.Departamento (id_Departamento) ON DELETE SET NULL
    )
END
GO

IF OBJECT_ID(N'dbo.Reclamo', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Reclamo (
        id_Reclamo              INT IDENTITY(1,1) NOT NULL,
        id_UsuarioDepartamento  INT               NOT NULL,
        Motivo                  VARCHAR(100)      NOT NULL,
        Categoria               VARCHAR(50)       NULL,
        Ubicacion               VARCHAR(100)      NOT NULL,
        Descripcion             VARCHAR(MAX)      NOT NULL,
        Observacion             VARCHAR(MAX)      NULL,
        estadoReclamo           VARCHAR(20)       NOT NULL,
        fechaReclamo            DATETIME          NOT NULL,
        CONSTRAINT PK_Reclamo PRIMARY KEY CLUSTERED (id_Reclamo),
        CONSTRAINT FK_Reclamo_Propietario FOREIGN KEY (id_UsuarioDepartamento) REFERENCES dbo.Propietario (id_Propietario)
    )
END
GO

/* ---------------------------------------------------------------------
   5. GASTOS Y EXPENSAS
   --------------------------------------------------------------------- */

IF OBJECT_ID(N'dbo.conceptoGasto', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.conceptoGasto (
        id_conceptoGasto  INT IDENTITY(1,1) NOT NULL,
        nombreConcepto    VARCHAR(100)      NOT NULL,
        descripcion       VARCHAR(250)      NULL,
        CONSTRAINT PK_conceptoGasto PRIMARY KEY CLUSTERED (id_conceptoGasto)
    )
END
GO

IF OBJECT_ID(N'dbo.Gasto_edificio', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Gasto_edificio (
        id_Gasto          INT IDENTITY(1,1) NOT NULL,
        Id_Edificio       INT               NOT NULL,
        id_conceptoGasto  INT               NOT NULL,
        descripcion       VARCHAR(250)      NULL,
        monto             DECIMAL(12,2)     NOT NULL,
        periodo           VARCHAR(20)       NOT NULL,
        fecha             DATE              NOT NULL,
        estado            VARCHAR(20)       NOT NULL,
        CONSTRAINT PK_Gasto_edificio PRIMARY KEY CLUSTERED (id_Gasto),
        CONSTRAINT FK_Gasto_edificio_Edificio FOREIGN KEY (Id_Edificio) REFERENCES dbo.Edificio (id_edificio),
        CONSTRAINT FK_Gasto_edificio_conceptoGasto FOREIGN KEY (id_conceptoGasto) REFERENCES dbo.conceptoGasto (id_conceptoGasto)
    )
END
GO

/* Cabecera de la liquidacion del edificio para un periodo (MM/aaaa) */
IF OBJECT_ID(N'dbo.Expensa', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Expensa (
        id_Expensa              INT IDENTITY(1,1) NOT NULL,
        id_Edificio             INT               NOT NULL,
        periodo                 VARCHAR(7)        NOT NULL,
        montoTotalGastos        DECIMAL(18,2)     NOT NULL,
        valorExpensaIndividual  DECIMAL(18,2)     NOT NULL,
        fechaEmision            DATE              NOT NULL,
        CONSTRAINT PK_Expensa PRIMARY KEY CLUSTERED (id_Expensa),
        CONSTRAINT FK_Expensa_Edificio FOREIGN KEY (id_Edificio) REFERENCES dbo.Edificio (id_edificio)
    )
END
GO

/* Boleta de cada departamento y control administrativo del pago */
IF OBJECT_ID(N'dbo.Detalle_Expensa', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Detalle_Expensa (
        id_DetalleExpensa  INT IDENTITY(1,1) NOT NULL,
        id_Expensa         INT               NOT NULL,
        id_Departamento    INT               NOT NULL,
        montoAPagar        DECIMAL(18,2)     NOT NULL,
        fechaVencimiento   DATE              NOT NULL,
        estadoPago         VARCHAR(20)       NOT NULL CONSTRAINT DF_DetalleExpensa_Estado DEFAULT 'Pendiente',
        fechaPago          DATE              NULL,
        metodoPago         VARCHAR(50)       NULL,
        CONSTRAINT PK_Detalle_Expensa PRIMARY KEY CLUSTERED (id_DetalleExpensa),
        CONSTRAINT FK_DetalleExpensa_Expensa FOREIGN KEY (id_Expensa) REFERENCES dbo.Expensa (id_Expensa),
        CONSTRAINT FK_DetalleExpensa_Departamento FOREIGN KEY (id_Departamento) REFERENCES dbo.Departamento (id_Departamento)
    )
END
GO

/* ---------------------------------------------------------------------
   6. PROCEDIMIENTO ALMACENADO (alta de usuarios desde la aplicacion)
   --------------------------------------------------------------------- */

CREATE OR ALTER PROCEDURE dbo.sp_InsertarUsuario
    @Nombre         VARCHAR(50),
    @Apellido       VARCHAR(50),
    @Dni            VARCHAR(20),
    @Telefono       VARCHAR(30),
    @Email          VARCHAR(100),
    @UsuarioSistema VARCHAR(50),
    @Contrasenia    VARCHAR(100),
    @IdPerfil       INT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Usuarios
        (Nombre, Apellido, Dni, Telefono, Email, UsuarioSistema, Contrasenia, IdPerfil, Estado, FechaAlta)
    VALUES
        (@Nombre, @Apellido, @Dni, @Telefono, @Email, @UsuarioSistema, @Contrasenia, @IdPerfil, 1, GETDATE());
END
GO

/* ---------------------------------------------------------------------
   7. DATOS INICIALES
   --------------------------------------------------------------------- */

/* 7.1 Perfiles (el codigo de la aplicacion espera estos tres, con estos Id) */
IF NOT EXISTS (SELECT 1 FROM dbo.Perfiles)
BEGIN
    SET IDENTITY_INSERT dbo.Perfiles ON
    INSERT INTO dbo.Perfiles (IdPerfil, NombrePerfil) VALUES (1, N'Administrador')
    INSERT INTO dbo.Perfiles (IdPerfil, NombrePerfil) VALUES (2, N'Encargado')
    INSERT INTO dbo.Perfiles (IdPerfil, NombrePerfil) VALUES (3, N'Propietario')
    SET IDENTITY_INSERT dbo.Perfiles OFF
END
GO

/* 7.2 Usuarios de prueba, uno por perfil (datos de relleno) */
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios)
BEGIN
    SET IDENTITY_INSERT dbo.Usuarios ON
    INSERT INTO dbo.Usuarios (IdUsuario, Nombre, Apellido, Dni, Telefono, Email, UsuarioSistema, Contrasenia, IdPerfil, Estado)
    VALUES (1, N'Admin',       N'Demo', N'00000001', N'0000000000', N'admin@consorapp.local',       N'admin',       N'1234', 1, 1)
    INSERT INTO dbo.Usuarios (IdUsuario, Nombre, Apellido, Dni, Telefono, Email, UsuarioSistema, Contrasenia, IdPerfil, Estado)
    VALUES (2, N'Encargado',   N'Demo', N'00000002', N'0000000000', N'encargado@consorapp.local',   N'encargado',   N'1234', 2, 1)
    INSERT INTO dbo.Usuarios (IdUsuario, Nombre, Apellido, Dni, Telefono, Email, UsuarioSistema, Contrasenia, IdPerfil, Estado)
    VALUES (3, N'Propietario', N'Demo', N'00000003', N'0000000000', N'propietario@consorapp.local', N'propietario', N'1234', 3, 1)
    SET IDENTITY_INSERT dbo.Usuarios OFF
END
GO

/* 7.3 Turnos de reserva */
IF NOT EXISTS (SELECT 1 FROM dbo.Turno)
BEGIN
    INSERT INTO dbo.Turno (nombreTurno, horaInicio, horaFin) VALUES (N'Mañana', '09:00:00', '14:00:00')
    INSERT INTO dbo.Turno (nombreTurno, horaInicio, horaFin) VALUES (N'Tarde',  '15:00:00', '20:00:00')
    INSERT INTO dbo.Turno (nombreTurno, horaInicio, horaFin) VALUES (N'Noche',  '21:00:00', '02:00:00')
END
GO

PRINT 'Base de datos consorAppDb lista.'
GO
