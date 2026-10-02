using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using ConsorApp.Entidades;

namespace ConsorApp.Datos
{
    public class HistorialExpensaDatos
    {
        private readonly string connectionString =
            @"Server=.\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";


        // =========================================================
        // OBTENER HISTORIAL DE EXPENSAS
        // =========================================================

        public List<HistorialExpensa> ObtenerHistorial()
        {
            var lista = new List<HistorialExpensa>();

            string query = @"
                SELECT 
                    de.Id_DetalleExpensa,
                    e.Id_Expensa,
                    d.Id_Departamento,
                    d.Piso,
                    d.Unidad,

                    p.Id_Propietario AS IdPropietario,
                    p.Id_Usuario AS IdUsuario,

                    u.Nombre + ' ' + u.Apellido AS NombrePropietario,

                    e.Periodo,

                    de.MontoAPagar,
                    de.EstadoPago,
                    de.FechaVencimiento,
                    de.FechaPago,
                    ISNULL(de.MetodoPago, '') AS MetodoPago,

                    p.FechaDesde AS FechaDesdePropietario,
                    p.FechaHasta AS FechaHastaPropietario

                FROM Detalle_Expensa de

                INNER JOIN Expensa e
                    ON de.Id_Expensa = e.Id_Expensa

                INNER JOIN Departamento d
                    ON de.Id_Departamento = d.Id_Departamento

                INNER JOIN Propietario p
                    ON p.Id_Departamento = de.Id_Departamento

                INNER JOIN Usuarios u
                    ON p.Id_Usuario = u.idUsuario

                WHERE 
                    p.FechaDesde <= 
                        ISNULL(de.FechaPago, de.FechaVencimiento)

                    AND
                    (
                        p.FechaHasta IS NULL
                        OR
                        p.FechaHasta >= 
                            ISNULL(de.FechaPago, de.FechaVencimiento)
                    )

                ORDER BY 
                    d.Piso,
                    d.Unidad,
                    ISNULL(de.FechaPago, de.FechaVencimiento) DESC;
            ";

            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                SqlCommand command =
                    new SqlCommand(query, connection);

                try
                {
                    connection.Open();

                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var historial = new HistorialExpensa
                            {
                                IdDetalleExpensa =
                                    Convert.ToInt32(
                                        reader["Id_DetalleExpensa"]),

                                IdExpensa =
                                    Convert.ToInt32(
                                        reader["Id_Expensa"]),

                                IdDepartamento =
                                    Convert.ToInt32(
                                        reader["Id_Departamento"]),

                                Piso =
                                    reader["Piso"]?.ToString()
                                    ?? string.Empty,

                                Unidad =
                                    reader["Unidad"]?.ToString()
                                    ?? string.Empty,

                                IdPropietario =
                                    Convert.ToInt32(
                                        reader["IdPropietario"]),

                                IdUsuario =
                                    Convert.ToInt32(
                                        reader["IdUsuario"]),

                                NombrePropietario =
                                    reader["NombrePropietario"]?.ToString()
                                    ?? string.Empty,

                                Periodo =
                                    reader["Periodo"]?.ToString()
                                    ?? string.Empty,

                                MontoAPagar =
                                    Convert.ToDecimal(
                                        reader["MontoAPagar"]),

                                EstadoPago =
                                    reader["EstadoPago"]?.ToString()
                                    ?? string.Empty,

                                FechaVencimiento =
                                    Convert.ToDateTime(
                                        reader["FechaVencimiento"]),

                                FechaPago =
                                    reader["FechaPago"] != DBNull.Value
                                    ? Convert.ToDateTime(
                                        reader["FechaPago"])
                                    : (DateTime?)null,

                                MetodoPago =
                                    reader["MetodoPago"]?.ToString()
                                    ?? string.Empty,

                                FechaDesdePropietario =
                                    Convert.ToDateTime(
                                        reader["FechaDesdePropietario"]),

                                FechaHastaPropietario =
                                    reader["FechaHastaPropietario"]
                                    != DBNull.Value
                                    ? Convert.ToDateTime(
                                        reader["FechaHastaPropietario"])
                                    : (DateTime?)null
                            };

                            lista.Add(historial);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Error en la capa de datos al obtener el historial de expensas: "
                        + ex.Message);
                }
            }

            return lista;
        }


        // =========================================================
        // OBTENER HISTORIAL DE UN DEPARTAMENTO
        // =========================================================

        public List<HistorialExpensa> ObtenerHistorialPorDepartamento(
            int idDepartamento)
        {
            var lista = new List<HistorialExpensa>();

            string query = @"
                SELECT 
                    de.Id_DetalleExpensa,
                    e.Id_Expensa,
                    d.Id_Departamento,
                    d.Piso,
                    d.Unidad,

                    p.Id_Propietario AS IdPropietario,
                    p.Id_Usuario AS IdUsuario,

                    u.Nombre + ' ' + u.Apellido AS NombrePropietario,

                    e.Periodo,

                    de.MontoAPagar,
                    de.EstadoPago,
                    de.FechaVencimiento,
                    de.FechaPago,
                    ISNULL(de.MetodoPago, '') AS MetodoPago,

                    p.FechaDesde AS FechaDesdePropietario,
                    p.FechaHasta AS FechaHastaPropietario

                FROM Detalle_Expensa de

                INNER JOIN Expensa e
                    ON de.Id_Expensa = e.Id_Expensa

                INNER JOIN Departamento d
                    ON de.Id_Departamento = d.Id_Departamento

                INNER JOIN Propietario p
                    ON p.Id_Departamento = de.Id_Departamento

                INNER JOIN Usuarios u
                    ON p.Id_Usuario = u.idUsuario

                WHERE 
                    d.Id_Departamento = @IdDepartamento

                    AND p.FechaDesde <= 
                        ISNULL(de.FechaPago, de.FechaVencimiento)

                    AND
                    (
                        p.FechaHasta IS NULL
                        OR
                        p.FechaHasta >= 
                            ISNULL(de.FechaPago, de.FechaVencimiento)
                    )

                ORDER BY 
                    ISNULL(de.FechaPago, de.FechaVencimiento) DESC;
            ";

            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.AddWithValue(
                    "@IdDepartamento",
                    idDepartamento);

                try
                {
                    connection.Open();

                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new HistorialExpensa
                            {
                                IdDetalleExpensa =
                                    Convert.ToInt32(
                                        reader["Id_DetalleExpensa"]),

                                IdExpensa =
                                    Convert.ToInt32(
                                        reader["Id_Expensa"]),

                                IdDepartamento =
                                    Convert.ToInt32(
                                        reader["Id_Departamento"]),

                                Piso =
                                    reader["Piso"]?.ToString()
                                    ?? string.Empty,

                                Unidad =
                                    reader["Unidad"]?.ToString()
                                    ?? string.Empty,

                                IdPropietario =
                                    Convert.ToInt32(
                                        reader["IdPropietario"]),

                                IdUsuario =
                                    Convert.ToInt32(
                                        reader["IdUsuario"]),

                                NombrePropietario =
                                    reader["NombrePropietario"]?.ToString()
                                    ?? string.Empty,

                                Periodo =
                                    reader["Periodo"]?.ToString()
                                    ?? string.Empty,

                                MontoAPagar =
                                    Convert.ToDecimal(
                                        reader["MontoAPagar"]),

                                EstadoPago =
                                    reader["EstadoPago"]?.ToString()
                                    ?? string.Empty,

                                FechaVencimiento =
                                    Convert.ToDateTime(
                                        reader["FechaVencimiento"]),

                                FechaPago =
                                    reader["FechaPago"] != DBNull.Value
                                    ? Convert.ToDateTime(
                                        reader["FechaPago"])
                                    : (DateTime?)null,

                                MetodoPago =
                                    reader["MetodoPago"]?.ToString()
                                    ?? string.Empty,

                                FechaDesdePropietario =
                                    Convert.ToDateTime(
                                        reader["FechaDesdePropietario"]),

                                FechaHastaPropietario =
                                    reader["FechaHastaPropietario"]
                                    != DBNull.Value
                                    ? Convert.ToDateTime(
                                        reader["FechaHastaPropietario"])
                                    : (DateTime?)null
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Error en la capa de datos al obtener el historial del departamento: "
                        + ex.Message);
                }
            }

            return lista;
        }
    }
}