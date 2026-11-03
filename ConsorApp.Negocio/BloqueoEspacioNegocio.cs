using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace ConsorApp.Negocio
{
    public class BloqueoEspacioNegocio
    {
        private readonly string cadenaConexion =
            "Server=.\\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

        private readonly AvisoNegocio _avisoNegocio = new AvisoNegocio();

        // Registra un nuevo bloqueo, cancela las reservas afectadas, les asigna el motivo y genera un aviso
        public void RegistrarBloqueo(int idEspacioComun, DateTime? fechaInicio, DateTime? fechaFin, string motivo)
        {
            if (!fechaInicio.HasValue || !fechaFin.HasValue)
            {
                throw new Exception("Debe especificar las fechas de inicio y fin del bloqueo.");
            }

            if (fechaInicio.Value > fechaFin.Value)
            {
                throw new Exception("La fecha de inicio no puede ser posterior a la fecha de fin.");
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                throw new Exception("Debe ingresar un motivo para el bloqueo.");
            }

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                using (SqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        // 1. Insertar el nuevo bloqueo
                        string queryBloqueo = @"
                            INSERT INTO BloqueoEspacio (id_espacioComun, fechaDesde, fechaHasta, motivo)
                            VALUES (@IdEspacio, @FechaDesde, @FechaHasta, @Motivo)";

                        using (SqlCommand cmdBloqueo = new SqlCommand(queryBloqueo, conexion, transaccion))
                        {
                            cmdBloqueo.Parameters.AddWithValue("@IdEspacio", idEspacioComun);
                            cmdBloqueo.Parameters.AddWithValue("@FechaDesde", fechaInicio.Value.Date);
                            cmdBloqueo.Parameters.AddWithValue("@FechaHasta", fechaFin.Value.Date);
                            cmdBloqueo.Parameters.AddWithValue("@Motivo", motivo.Trim());
                            cmdBloqueo.ExecuteNonQuery();
                        }

                        // 2. Obtener las reservas afectadas (Cambiamos el nombre del campo a fechaReserva para evitar el error)
                        string queryObtenerReservasAfectadas = @"
                            SELECT r.id_Reserva, r.id_Propietario, p.Id_Usuario, p.id_Departamento, r.fechaReserva
                            FROM Reserva r
                            INNER JOIN Propietario p ON r.id_Propietario = p.id_Propietario
                            WHERE r.id_EspacioComun = @IdEspacio 
                              AND r.estadoReserva = 'Confirmada'
                              AND r.fechaReserva >= @FechaDesde 
                              AND r.fechaReserva <= @FechaHasta";

                        var reservasAfectadas = new System.Collections.Generic.List<(int idReserva, int idUsuario, int? idEdificio, int idDepartamento, DateTime fechaReserva)>();

                        using (SqlCommand cmdObtener = new SqlCommand(queryObtenerReservasAfectadas, conexion, transaccion))
                        {
                            cmdObtener.Parameters.AddWithValue("@IdEspacio", idEspacioComun);
                            cmdObtener.Parameters.AddWithValue("@FechaDesde", fechaInicio.Value.Date);
                            cmdObtener.Parameters.AddWithValue("@FechaHasta", fechaFin.Value.Date);

                            using (SqlDataReader reader = cmdObtener.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    int idReserva = Convert.ToInt32(reader["id_Reserva"]);
                                    int idUsuario = reader["Id_Usuario"] != DBNull.Value ? Convert.ToInt32(reader["Id_Usuario"]) : 0;
                                    int idDepartamento = Convert.ToInt32(reader["id_Departamento"]);
                                    DateTime fechaReserva = Convert.ToDateTime(reader["fechaReserva"]);

                                    reservasAfectadas.Add((idReserva, idUsuario, null, idDepartamento, fechaReserva));
                                }
                            }
                        }

                        // 3. Cancelar las reservas afectadas
                        string queryCancelarReservas = @"
                            UPDATE Reserva 
                            SET estadoReserva = 'Cancelada', 
                                motivoCancelacion = @MotivoCancelacion
                            WHERE id_EspacioComun = @IdEspacio 
                              AND estadoReserva = 'Confirmada'
                              AND fechaReserva >= @FechaDesde 
                              AND fechaReserva <= @FechaHasta";

                        string motivoCancelacionTexto = $"Cancelado por bloqueo de espacio: {motivo.Trim()}";

                        using (SqlCommand cmdReservas = new SqlCommand(queryCancelarReservas, conexion, transaccion))
                        {
                            cmdReservas.Parameters.AddWithValue("@IdEspacio", idEspacioComun);
                            cmdReservas.Parameters.AddWithValue("@FechaDesde", fechaInicio.Value.Date);
                            cmdReservas.Parameters.AddWithValue("@FechaHasta", fechaFin.Value.Date);
                            cmdReservas.Parameters.AddWithValue("@MotivoCancelacion", motivoCancelacionTexto);
                            cmdReservas.ExecuteNonQuery();
                        }

                        transaccion.Commit();

                        // 4. Enviar los avisos correspondientes a cada propietario afectado
                        foreach (var res in reservasAfectadas)
                        {
                            if (res.idUsuario > 0)
                            {
                                string tituloAviso = "Reserva cancelada por mantenimiento";
                                string mensajeAviso = $"Lamentamos informarle que su reserva para el día {res.fechaReserva:dd/MM/yyyy} ha sido cancelada debido a un bloqueo del espacio común. Motivo: {motivo.Trim()}";

                                try
                                {
                                    _avisoNegocio.RegistrarAviso(tituloAviso, mensajeAviso, res.idUsuario, 1, res.idDepartamento);
                                }
                                catch
                                {
                                    // Si falla la creación de un aviso individual, no bloqueamos el flujo
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }

        // Obtiene todos los bloqueos registrados para mostrarlos en la grilla del admin
        public DataTable ObtenerBloqueosAdmin()
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                string query = @"
                    SELECT 
                        b.id_Bloqueo AS IdBloqueo,
                        b.id_espacioComun AS IdEspacioComun,
                        e.nombre AS Espacio,
                        CONVERT(VARCHAR(10), b.fechaDesde, 103) AS FechaDesde,
                        CONVERT(VARCHAR(10), b.fechaHasta, 103) AS FechaHasta,
                        b.motivo AS Motivo
                    FROM BloqueoEspacio b
                    INNER JOIN espacioComun e ON b.id_espacioComun = e.id_espacioComun
                    ORDER BY b.fechaDesde DESC";

                SqlDataAdapter adaptador = new SqlDataAdapter(query, conexion);
                DataTable dt = new DataTable();
                adaptador.Fill(dt);
                return dt;
            }
        }

        // Modifica un bloqueo existente seleccionado
        public void ModificarBloqueo(int idBloqueo, int idEspacioComun, DateTime? fechaInicio, DateTime? fechaFin, string motivo)
        {
            if (!fechaInicio.HasValue || !fechaFin.HasValue)
            {
                throw new Exception("Debe especificar las fechas de inicio y fin del bloqueo.");
            }

            if (fechaInicio.Value > fechaFin.Value)
            {
                throw new Exception("La fecha de inicio no puede ser posterior a la fecha de fin.");
            }

            if (string.IsNullOrWhiteSpace(motivo))
            {
                throw new Exception("Debe ingresar un motivo para el bloqueo.");
            }

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = @"
                    UPDATE BloqueoEspacio 
                    SET id_espacioComun = @IdEspacio, 
                        fechaDesde = @FechaDesde, 
                        fechaHasta = @FechaHasta, 
                        motivo = @Motivo
                    WHERE id_Bloqueo = @IdBloqueo";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdBloqueo", idBloqueo);
                comando.Parameters.AddWithValue("@IdEspacio", idEspacioComun);
                comando.Parameters.AddWithValue("@FechaDesde", fechaInicio.Value.Date);
                comando.Parameters.AddWithValue("@FechaHasta", fechaFin.Value.Date);
                comando.Parameters.AddWithValue("@Motivo", motivo.Trim());

                comando.ExecuteNonQuery();
            }
        }

        // Elimina o cancela un bloqueo registrado
        public void EliminarBloqueo(int idBloqueo)
        {
            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();
                string query = "DELETE FROM BloqueoEspacio WHERE id_Bloqueo = @IdBloqueo";

                SqlCommand comando = new SqlCommand(query, conexion);
                comando.Parameters.AddWithValue("@IdBloqueo", idBloqueo);

                comando.ExecuteNonQuery();
            }
        }
    }
}