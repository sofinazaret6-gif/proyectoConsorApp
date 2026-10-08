using System;
using System.Collections.Generic;
using System.Data;
using ConsorApp.Entidades;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;

namespace ConsorApp.Datos
{
    public class ExpensasDatos
    {
        private readonly string connectionString =
            @"Server=.\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

        // ============================================================
        // OBTENER TODAS LAS EXPENSAS
        // ============================================================

        public List<Expensa> ObtenerExpensas()
        {
            List<Expensa> lista = new List<Expensa>();

            string query = @"
                SELECT
                    Id_Expensa,
                    Id_Edificio,
                    Periodo,
                    MontoTotalGastos,
                    ValorExpensaIndividual,
                    FechaEmision
                FROM Expensa
                ORDER BY FechaEmision DESC";

            using (SqlConnection conexion =
                   new SqlConnection(connectionString))
            {
                using (SqlCommand comando =
                       new SqlCommand(query, conexion))
                {
                    conexion.Open();

                    using (SqlDataReader reader =
                           comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Expensa
                            {
                                Id_Expensa =
                                    Convert.ToInt32(
                                        reader["Id_Expensa"]),

                                Id_Edificio =
                                    Convert.ToInt32(
                                        reader["Id_Edificio"]),

                                Periodo =
                                    reader["Periodo"]?.ToString()
                                    ?? string.Empty,

                                MontoTotalGastos =
                                    Convert.ToDecimal(
                                        reader["MontoTotalGastos"]),

                                ValorExpensaIndividual =
                                    Convert.ToDecimal(
                                        reader["ValorExpensaIndividual"]),

                                FechaEmision =
                                    Convert.ToDateTime(
                                        reader["FechaEmision"])
                            });
                        }
                    }
                }
            }

            return lista;
        }


        // ============================================================
        // BUSCAR EXPENSA POR PERÍODO
        // ============================================================

        public Expensa? ObtenerPorPeriodo(string periodo)
        {
            string query = @"
                SELECT
                    Id_Expensa,
                    Id_Edificio,
                    Periodo,
                    MontoTotalGastos,
                    ValorExpensaIndividual,
                    FechaEmision
                FROM Expensa
                WHERE Periodo = @Periodo";

            using (SqlConnection conexion =
                   new SqlConnection(connectionString))
            {
                using (SqlCommand comando =
                       new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue(
                        "@Periodo",
                        periodo);

                    conexion.Open();

                    using (SqlDataReader reader =
                           comando.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Expensa
                            {
                                Id_Expensa =
                                    Convert.ToInt32(
                                        reader["Id_Expensa"]),

                                Id_Edificio =
                                    Convert.ToInt32(
                                        reader["Id_Edificio"]),

                                Periodo =
                                    reader["Periodo"]?.ToString()
                                    ?? string.Empty,

                                MontoTotalGastos =
                                    Convert.ToDecimal(
                                        reader["MontoTotalGastos"]),

                                ValorExpensaIndividual =
                                    Convert.ToDecimal(
                                        reader["ValorExpensaIndividual"]),

                                FechaEmision =
                                    Convert.ToDateTime(
                                        reader["FechaEmision"])
                            };
                        }
                    }
                }
            }

            return null;
        }


        // ============================================================
        // OBTENER DEPARTAMENTOS QUE TIENEN PROPIETARIO ACTUAL
        // ============================================================

        public List<Departamento> ObtenerDepartamentosConPropietario()
        {
            List<Departamento> lista =
                new List<Departamento>();

            string query = @"
        SELECT DISTINCT
            d.Id_Departamento,
            d.Id_Edificio,
            d.Piso,
            d.Unidad
        FROM Departamento d
        INNER JOIN Propietario p
            ON d.Id_Departamento = p.Id_Departamento
        WHERE p.Estado = 1
          AND p.FechaHasta IS NULL
        ORDER BY d.Piso, d.Unidad";

            using (SqlConnection conexion =
                   new SqlConnection(connectionString))
            {
                using (SqlCommand comando =
                       new SqlCommand(query, conexion))
                {
                    conexion.Open();

                    using (SqlDataReader reader =
                           comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Departamento
                            {
                                IdDepartamento =
                                    Convert.ToInt32(
                                        reader["Id_Departamento"]),

                                IdEdificio =
                                    Convert.ToInt32(
                                        reader["Id_Edificio"]),

                                Piso =
                                    reader["Piso"]?.ToString()
                                    ?? string.Empty,

                                Unidad =
                                    reader["Unidad"]?.ToString()
                                    ?? string.Empty
                            });
                        }
                    }
                }
            }

            return lista;
        }


        // ============================================================
        // GUARDAR LIQUIDACIÓN COMPLETA
        // CREA EXPENSA + DETALLES
        // ============================================================

        public int GuardarLiquidacionCompleta(
            Expensa expensa,
            List<int> departamentos,
            DateTime fechaVencimiento)
        {
            using (SqlConnection conexion =
                   new SqlConnection(connectionString))
            {
                conexion.Open();

                SqlTransaction transaccion =
                    conexion.BeginTransaction();

                try
                {
                    // ------------------------------------------------
                    // 1. INSERTAR EXPENSA
                    // ------------------------------------------------

                    string queryExpensa = @"
                        INSERT INTO Expensa
                        (
                            Id_Edificio,
                            Periodo,
                            MontoTotalGastos,
                            ValorExpensaIndividual,
                            FechaEmision
                        )
                        VALUES
                        (
                            @Id_Edificio,
                            @Periodo,
                            @MontoTotalGastos,
                            @ValorExpensaIndividual,
                            @FechaEmision
                        );

                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

                    int idExpensa;

                    using (SqlCommand comando =
                           new SqlCommand(
                               queryExpensa,
                               conexion,
                               transaccion))
                    {
                        comando.Parameters.AddWithValue(
                            "@Id_Edificio",
                            expensa.Id_Edificio);

                        comando.Parameters.AddWithValue(
                            "@Periodo",
                            expensa.Periodo);

                        comando.Parameters.AddWithValue(
                            "@MontoTotalGastos",
                            expensa.MontoTotalGastos);

                        comando.Parameters.AddWithValue(
                            "@ValorExpensaIndividual",
                            expensa.ValorExpensaIndividual);

                        comando.Parameters.AddWithValue(
                            "@FechaEmision",
                            expensa.FechaEmision);

                        idExpensa =
                            Convert.ToInt32(
                                comando.ExecuteScalar());
                    }


                    // ------------------------------------------------
                    // 2. CREAR DETALLE PARA CADA DEPARTAMENTO
                    // ------------------------------------------------

                    string queryDetalle = @"
                        INSERT INTO Detalle_Expensa
                        (
                            Id_Expensa,
                            Id_Departamento,
                            MontoAPagar,
                            FechaVencimiento,
                            EstadoPago,
                            FechaPago,
                            MetodoPago
                        )
                        VALUES
                        (
                            @Id_Expensa,
                            @Id_Departamento,
                            @MontoAPagar,
                            @FechaVencimiento,
                            @EstadoPago,
                            NULL,
                            NULL
                        )";

                    foreach (int idDepartamento
                             in departamentos)
                    {
                        using (SqlCommand comando =
                               new SqlCommand(
                                   queryDetalle,
                                   conexion,
                                   transaccion))
                        {
                            comando.Parameters.AddWithValue(
                                "@Id_Expensa",
                                idExpensa);

                            comando.Parameters.AddWithValue(
                                "@Id_Departamento",
                                idDepartamento);

                            comando.Parameters.AddWithValue(
                                "@MontoAPagar",
                                expensa.ValorExpensaIndividual);

                            comando.Parameters.AddWithValue(
                                "@FechaVencimiento",
                                fechaVencimiento);

                            comando.Parameters.AddWithValue(
                                "@EstadoPago",
                                "Pendiente");

                            comando.ExecuteNonQuery();
                        }
                    }


                    // ------------------------------------------------
                    // 3. CONFIRMAR TODO
                    // ------------------------------------------------

                    transaccion.Commit();

                    return idExpensa;
                }
                catch
                {
                    transaccion.Rollback();
                    throw;
                }
            }
        }


        // ============================================================
        // OBTENER DETALLES DE UNA EXPENSA
        // ============================================================

        public List<Detalle_Expensa> ObtenerDetalles(
            int idExpensa)
        {
            List<Detalle_Expensa> lista =
                new List<Detalle_Expensa>();
            string query = @"
                SELECT
                      de.Id_DetalleExpensa,
                      de.Id_Expensa,
                      de.Id_Departamento,
                       d.Piso,
                      d.Unidad,
                      de.MontoAPagar,
                      de.FechaVencimiento,
                      de.EstadoPago,
                      de.FechaPago,
                      de.MetodoPago
                     FROM Detalle_Expensa de
                    INNER JOIN Departamento d
                    ON d.Id_Departamento = de.Id_Departamento
                    WHERE de.Id_Expensa = @Id_Expensa
                    ORDER BY d.Piso, d.Unidad";
            using (SqlConnection conexion =
                   new SqlConnection(connectionString))
            {
                using (SqlCommand comando =
                       new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue(
                        "@Id_Expensa",
                        idExpensa);

                    conexion.Open();

                    using (SqlDataReader reader =
                           comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Detalle_Expensa
                            {
                                Id_DetalleExpensa =
                                    Convert.ToInt32(
                                        reader["Id_DetalleExpensa"]),

                                Id_Expensa =
                                    Convert.ToInt32(
                                        reader["Id_Expensa"]),

                                Id_Departamento =
                                    Convert.ToInt32(
                                        reader["Id_Departamento"]),

                                // ===== NUEVO =====
                                Departamento = new Departamento
                                {
                                    IdDepartamento = Convert.ToInt32(reader["Id_Departamento"]),
                                    Piso = reader["Piso"]?.ToString() ?? string.Empty,
                                    Unidad = reader["Unidad"]?.ToString() ?? string.Empty
                                },
                                // =================

                                MontoAPagar =
                                    Convert.ToDecimal(
                                        reader["MontoAPagar"]),

                                FechaVencimiento =
                                    Convert.ToDateTime(
                                        reader["FechaVencimiento"]),

                                EstadoPago =
                                    reader["EstadoPago"]?.ToString()
                                    ?? "Pendiente",

                                FechaPago =
                                    reader["FechaPago"] == DBNull.Value
                                        ? null
                                        : Convert.ToDateTime(reader["FechaPago"]),

                                MetodoPago =
                                    reader["MetodoPago"] == DBNull.Value
                                        ? null
                                        : reader["MetodoPago"].ToString()
                            });
                        }
                    }
                }
            }

            return lista;
        }


        // ============================================================
        // REGISTRAR PAGO
        // ============================================================

        public void ActualizarPago(
            int idDetalle,
            string estadoPago,
            DateTime? fechaPago,
            string? metodoPago)
        {
            string query = @"
                UPDATE Detalle_Expensa
                SET
                    EstadoPago = @EstadoPago,
                    FechaPago = @FechaPago,
                    MetodoPago = @MetodoPago
                WHERE Id_DetalleExpensa = @Id_DetalleExpensa";

            using (SqlConnection conexion =
                   new SqlConnection(connectionString))
            {
                using (SqlCommand comando =
                       new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue(
                        "@Id_DetalleExpensa",
                        idDetalle);

                    comando.Parameters.AddWithValue(
                        "@EstadoPago",
                        estadoPago);

                    comando.Parameters.AddWithValue(
                        "@FechaPago",
                        fechaPago.HasValue
                            ? fechaPago.Value
                            : (object)DBNull.Value);

                    comando.Parameters.AddWithValue(
                        "@MetodoPago",
                        string.IsNullOrWhiteSpace(metodoPago)
                            ? (object)DBNull.Value
                            : metodoPago);

                    conexion.Open();

                    comando.ExecuteNonQuery();
                }
            }
        }

        // ============================================================
        // OBTENER TOTAL DE GASTOS DE UN PERÍODO
        // ============================================================

        public decimal ObtenerTotalGastosPorPeriodo(string periodo)
        {
            string query = @"
        SELECT ISNULL(SUM(monto), 0)
        FROM Gasto_Edificio
        WHERE periodo = @Periodo
          AND estado <> 'ANULADO'";

            using (SqlConnection conexion =
                   new SqlConnection(connectionString))
            {
                using (SqlCommand comando =
                       new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue(
                        "@Periodo",
                        periodo);

                    conexion.Open();

                    object resultado = comando.ExecuteScalar();

                    return Convert.ToDecimal(resultado);
                }
            }
        }

        // ============================================================
        // OBTENER CANTIDAD DE DEPARTAMENTOS CON PROPIETARIO ACTUAL
        // ============================================================

        public int ObtenerCantidadDepartamentosConPropietario()
        {
            string query = @"
        SELECT COUNT(DISTINCT Id_Departamento)
        FROM Propietario
        WHERE Estado = 1
          AND FechaHasta IS NULL";

            using (SqlConnection conexion =
                   new SqlConnection(connectionString))
            {
                using (SqlCommand comando =
                       new SqlCommand(query, conexion))
                {
                    conexion.Open();

                    object resultado = comando.ExecuteScalar();

                    return Convert.ToInt32(resultado);
                }
            }
        }
    }
}