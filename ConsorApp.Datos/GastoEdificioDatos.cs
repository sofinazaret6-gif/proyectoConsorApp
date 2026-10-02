using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ConsorApp.Entidades;

namespace ConsorApp.Datos
{
    public class GastoEdificioDatos
    {
        private readonly string cadenaConexion =
            "Server=.\\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

        public List<GastoEdificio> ObtenerGastos()
        {
            List<GastoEdificio> lista = new List<GastoEdificio>();

            string query = @"
                SELECT
                    g.id_Gasto,
                    g.id_Edificio,
                    g.Id_conceptoGasto,
                    g.descripcion,
                    g.monto,
                    g.periodo,
                    g.fecha,
                    g.estado,
                    c.nombreConcepto
                FROM Gasto_Edificio g
                INNER JOIN conceptoGasto c
                    ON g.Id_conceptoGasto = c.id_conceptoGasto
                WHERE g.estado <> 'ANULADO'
                ORDER BY g.fecha DESC";

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                using (SqlCommand comando = new SqlCommand(query, conexion))
                using (SqlDataReader reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new GastoEdificio
                        {
                            id_Gasto = Convert.ToInt32(reader["id_Gasto"]),
                            id_Edificio = Convert.ToInt32(reader["id_Edificio"]),
                            Id_conceptoGasto = Convert.ToInt32(reader["Id_conceptoGasto"]),

                            descripcion = reader["descripcion"] == DBNull.Value
                                ? ""
                                : reader["descripcion"].ToString() ?? "",

                            monto = Convert.ToDecimal(reader["monto"]),
                            periodo = reader["periodo"].ToString() ?? "",
                            fecha = Convert.ToDateTime(reader["fecha"]),
                            estado = reader["estado"].ToString() ?? "",

                            ConceptoGasto = new conceptoGasto
                            {
                                id_conceptoGasto =
                                    Convert.ToInt32(reader["Id_conceptoGasto"]),

                                nombreConcepto =
                                    reader["nombreConcepto"].ToString() ?? ""
                            }
                        });
                    }
                }
            }

            return lista;
        }

        public List<GastoEdificio> ObtenerGastosPorPeriodo(string periodo)
        {
            List<GastoEdificio> lista = new List<GastoEdificio>();

            string query = @"
                SELECT
                    g.id_Gasto,
                    g.id_Edificio,
                    g.Id_conceptoGasto,
                    g.descripcion,
                    g.monto,
                    g.periodo,
                    g.fecha,
                    g.estado,
                    c.nombreConcepto
                FROM Gasto_Edificio g
                INNER JOIN conceptoGasto c
                    ON g.Id_conceptoGasto = c.id_conceptoGasto
                WHERE g.periodo = @periodo
                  AND g.estado <> 'ANULADO'
                ORDER BY g.fecha DESC";

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@periodo", periodo);

                    using (SqlDataReader reader = comando.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new GastoEdificio
                            {
                                id_Gasto = Convert.ToInt32(reader["id_Gasto"]),
                                id_Edificio = Convert.ToInt32(reader["id_Edificio"]),
                                Id_conceptoGasto = Convert.ToInt32(reader["Id_conceptoGasto"]),

                                descripcion = reader["descripcion"] == DBNull.Value
                                    ? ""
                                    : reader["descripcion"].ToString() ?? "",

                                monto = Convert.ToDecimal(reader["monto"]),
                                periodo = reader["periodo"].ToString() ?? "",
                                fecha = Convert.ToDateTime(reader["fecha"]),
                                estado = reader["estado"].ToString() ?? "",

                                ConceptoGasto = new conceptoGasto
                                {
                                    id_conceptoGasto =
                                        Convert.ToInt32(reader["Id_conceptoGasto"]),

                                    nombreConcepto =
                                        reader["nombreConcepto"].ToString() ?? ""
                                }
                            });
                        }
                    }
                }
            }

            return lista;
        }

        public void InsertarGasto(GastoEdificio gasto)
        {
            string query = @"
                INSERT INTO Gasto_Edificio
                (
                    id_Edificio,
                    Id_conceptoGasto,
                    descripcion,
                    monto,
                    periodo,
                    fecha,
                    estado
                )
                VALUES
                (
                    @id_Edificio,
                    @Id_conceptoGasto,
                    @descripcion,
                    @monto,
                    @periodo,
                    @fecha,
                    @estado
                )";

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue(
                        "@id_Edificio",
                        gasto.id_Edificio);

                    comando.Parameters.AddWithValue(
                        "@Id_conceptoGasto",
                        gasto.Id_conceptoGasto);

                    comando.Parameters.AddWithValue(
                        "@descripcion",
                        string.IsNullOrWhiteSpace(gasto.descripcion)
                            ? (object)DBNull.Value
                            : gasto.descripcion);

                    comando.Parameters.AddWithValue(
                        "@monto",
                        gasto.monto);

                    comando.Parameters.AddWithValue(
                        "@periodo",
                        gasto.periodo);

                    comando.Parameters.AddWithValue(
                        "@fecha",
                        gasto.fecha);

                    comando.Parameters.AddWithValue(
                        "@estado",
                        gasto.estado);

                    comando.ExecuteNonQuery();
                }
            }
        }

        public decimal ObtenerTotalPorPeriodo(string periodo)
        {
            string query = @"
                SELECT ISNULL(SUM(monto), 0)
                FROM Gasto_Edificio
                WHERE periodo = @periodo
                  AND estado <> 'ANULADO'";

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@periodo", periodo);

                    object resultado = comando.ExecuteScalar();

                    return Convert.ToDecimal(resultado);
                }
            }
        }

        public void AnularGasto(int idGasto)
        {
            string query = @"
                UPDATE Gasto_Edificio
                SET estado = 'ANULADO'
                WHERE id_Gasto = @id_Gasto";

            using (SqlConnection conexion = new SqlConnection(cadenaConexion))
            {
                conexion.Open();

                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue(
                        "@id_Gasto",
                        idGasto);

                    comando.ExecuteNonQuery();
                }
            }
        }
    }
}