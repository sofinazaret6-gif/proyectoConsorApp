using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using ConsorApp.Entidades;

namespace ConsorApp.Datos
{
    public class AvisoDatos
    {
        private readonly string connectionString =
            @"Server=.\SQLEXPRESS;Database=consorAppDb;Integrated Security=True;TrustServerCertificate=True;";


        // =========================================================
        // OBTENER TODOS LOS AVISOS
        // =========================================================

        public List<Aviso> ObtenerAvisos()
        {
            var listaAvisos = new List<Aviso>();

            string query = "SELECT id_Aviso, id_usuario, titulo, mensaje, fechaPublicacion, id_edificio, id_Departamento, estado FROM Aviso ORDER BY fechaPublicacion DESC";

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
                            Aviso aviso = new Aviso
                            {
                                id_Aviso =
                                    Convert.ToInt32(
                                        reader["id_Aviso"]),

                                id_usuario =
                                    Convert.ToInt32(
                                        reader["id_usuario"]),

                                titulo =
                                    reader["titulo"]?.ToString()
                                    ?? string.Empty,

                                mensaje =
                                    reader["mensaje"]?.ToString()
                                    ?? string.Empty,

                                fechaPublicacion =
                                    Convert.ToDateTime(
                                        reader["fechaPublicacion"]),

                                id_edificio =
                                    reader["id_edificio"] != DBNull.Value
                                    ? Convert.ToInt32(
                                        reader["id_edificio"])
                                    : (int?)null,

                                id_Departamento =
                                    reader["id_Departamento"] != DBNull.Value
                                    ? Convert.ToInt32(
                                        reader["id_Departamento"])
                                    : (int?)null,
                                estado = Convert.ToInt32(reader["estado"])
                            };

                            listaAvisos.Add(aviso);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Error en la capa de datos al obtener los avisos: "
                        + ex.Message);
                }
            }

            return listaAvisos;
        }


        // =========================================================
        // OBTENER AVISOS PARA UN PROPIETARIO
        // =========================================================

        public List<Aviso> ObtenerAvisosParaPropietario(
            int idUsuario)
        {
            var listaAvisos = new List<Aviso>();

            string query = @"
    SELECT
        a.id_Aviso,
        a.id_usuario,
        a.titulo,
        a.mensaje,
        a.fechaPublicacion,
        a.id_edificio,
        a.id_Departamento

    FROM Aviso a

    INNER JOIN Propietario p
        ON p.Id_Usuario = @IdUsuario
        AND p.fechaHasta IS NULL
        AND p.estado = 1

    INNER JOIN Departamento d
        ON d.id_Departamento = p.Id_Departamento

    WHERE
        a.estado = 1
        AND
        (
            -- Aviso para todo el edificio
            (
                a.id_edificio = d.id_Edificio
                AND a.id_Departamento IS NULL
            )

            OR

            -- Aviso para este departamento
            (
                a.id_Departamento = p.Id_Departamento
            )
        )

    ORDER BY a.fechaPublicacion DESC";


            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.AddWithValue(
                    "@IdUsuario",
                    idUsuario);

                try
                {
                    connection.Open();

                    using (SqlDataReader reader =
                           command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Aviso aviso = new Aviso
                            {
                                id_Aviso =
                                    Convert.ToInt32(
                                        reader["id_Aviso"]),

                                id_usuario =
                                    Convert.ToInt32(
                                        reader["id_usuario"]),

                                titulo =
                                    reader["titulo"]?.ToString()
                                    ?? string.Empty,

                                mensaje =
                                    reader["mensaje"]?.ToString()
                                    ?? string.Empty,

                                fechaPublicacion =
                                    Convert.ToDateTime(
                                        reader["fechaPublicacion"]),

                                id_edificio =
                                    reader["id_edificio"] != DBNull.Value
                                    ? Convert.ToInt32(
                                        reader["id_edificio"])
                                    : (int?)null,

                                id_Departamento =
                                    reader["id_Departamento"] != DBNull.Value
                                    ? Convert.ToInt32(
                                        reader["id_Departamento"])
                                    : (int?)null
                            };

                            listaAvisos.Add(aviso);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Error en la capa de datos al obtener los avisos del propietario: "
                        + ex.Message);
                }
            }

            return listaAvisos;
        }


        // =========================================================
        // INSERTAR AVISO
        // =========================================================

        public void InsertarAviso(Aviso aviso)
        {
            string query = @"
                INSERT INTO Aviso
                (
                    id_usuario,
                    titulo,
                    mensaje,
                    fechaPublicacion,
                    id_edificio,
                    id_Departamento
                )
                VALUES
                (
                    @id_usuario,
                    @titulo,
                    @mensaje,
                    @fechaPublicacion,
                    @id_edificio,
                    @id_Departamento
                )";


            using (SqlConnection connection =
                   new SqlConnection(connectionString))
            {
                SqlCommand command =
                    new SqlCommand(query, connection);

                command.Parameters.AddWithValue(
                    "@id_usuario",
                    aviso.id_usuario);

                command.Parameters.AddWithValue(
                    "@titulo",
                    aviso.titulo);

                command.Parameters.AddWithValue(
                    "@mensaje",
                    aviso.mensaje);

                command.Parameters.AddWithValue(
                    "@fechaPublicacion",
                    aviso.fechaPublicacion);

                command.Parameters.AddWithValue(
                    "@id_edificio",
                    (object?)aviso.id_edificio
                    ?? DBNull.Value);

                command.Parameters.AddWithValue(
                    "@id_Departamento",
                    (object?)aviso.id_Departamento
                    ?? DBNull.Value);


                try
                {
                    connection.Open();

                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Error en la capa de datos al registrar el aviso: "
                        + ex.Message);
                }
            }
        }


        // =========================================================
        // ELIMINAR AVISO
        // =========================================================

        public void ArchivarAviso(int idAviso)
        {
            string query = "UPDATE Aviso SET estado = 0 WHERE id_Aviso = @id_Aviso";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id_Aviso", idAviso);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Error en la capa de datos al archivar el aviso: " + ex.Message
                    );
                }
            }
        }
    }
}