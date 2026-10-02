using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using ConsorApp.Entidades;
using Microsoft.Data.SqlClient;

namespace ConsorApp.Datos
{
    public class ConceptoGastoDatos
    {
        private readonly string connectionString =
            @"Server=.\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";

        public List<conceptoGasto> ObtenerConceptos()
        {
            List<conceptoGasto> lista = new List<conceptoGasto>();

            string query = @"
                SELECT 
                    id_conceptoGasto,
                    nombreConcepto,
                    descripcion
                FROM conceptoGasto
                ORDER BY nombreConcepto";

            using (SqlConnection conexion = new SqlConnection(connectionString))
            {
                conexion.Open();

                using (SqlCommand comando = new SqlCommand(query, conexion))
                using (SqlDataReader reader = comando.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new conceptoGasto
                        {
                            id_conceptoGasto = Convert.ToInt32(reader["id_conceptoGasto"]),
                            nombreConcepto = reader["nombreConcepto"].ToString() ?? "",
                            descripcion = reader["descripcion"] == DBNull.Value
                                ? ""
                                : reader["descripcion"].ToString() ?? ""
                        });
                    }
                }
            }

            return lista;
        }

        public void InsertarConcepto(conceptoGasto concepto)
        {
            string query = @"
                INSERT INTO conceptoGasto
                (
                    nombreConcepto,
                    descripcion
                )
                VALUES
                (
                    @nombreConcepto,
                    @descripcion
                )";

            using (SqlConnection conexion = new SqlConnection(connectionString))
            {
                conexion.Open();

                using (SqlCommand comando = new SqlCommand(query, conexion))
                {
                    comando.Parameters.AddWithValue("@nombreConcepto", concepto.nombreConcepto);
                    comando.Parameters.AddWithValue(
                        "@descripcion",
                        string.IsNullOrWhiteSpace(concepto.descripcion)
                            ? (object)DBNull.Value
                            : concepto.descripcion
                    );

                    comando.ExecuteNonQuery();
                }
            }
        }
    }
}