using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using MvcCoreAWSDynamoDb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MvcCoreAWSDynamoDb.Services
{
    public class ServiceAWSDynamoDb
    {
        private DynamoDBContext context;

        public ServiceAWSDynamoDb()
        {
            AmazonDynamoDBClient client = new AmazonDynamoDBClient();
            this.context = new DynamoDBContext(client);
        }

        public async Task CreateUsuario(Usuario usuario)
        {
            await this.context.SaveAsync<Usuario>(usuario);
        }

        public async Task<List<Usuario>> GetUsuarios()
        {
            //PRIMERO DEBEMOS RECUPERAR LA TABLA
            //LA RECUPERACION DE LA TABLA ES SUPER SENCILLA
            //PARA RECUPERAR LA TABLA, BASTA CON HABER MAPEADO
            //EL MODEL CON [DynamoDBTable]
            var tabla = this.context.GetTargetTable<Usuario>();
            var scanOptions = new ScanOperationConfig();
            //scanOptions.PaginationToken = "";
            var results = tabla.Scan(scanOptions);
            //LOS DATOS QUE RECUPERAMOS SON Document
            //Y DEVUELVE UNA COLECCION
            List<Document> data = await results.GetNextSetAsync();
            //DEBEMOS TRANSFORMAR DICHOS DATOS A SU TIPADO
            //ESTO ES AUTOMATICO MEDIANTE UN METODO
            IEnumerable<Usuario> usuarios =
                this.context.FromDocuments<Usuario>(data);
            return usuarios.ToList();
        }

        public async Task<Usuario> FindUsuario(int idusuario)
        {
            //SI ESTAMOS BUSCANDO POR PARTITION KEY (PRIMARY KEY)
            //HASHKEY SOLAMENTE DEBEMOS HACERLO CON LOAD
            //ESTO ES EQUIVALENTE A BUSCAR CON CONSULTA
            return await this.context.LoadAsync<Usuario>(idusuario);
        }

        public async Task DeleteUsuario(int idusuario)
        {
            await this.context.DeleteAsync<Usuario>(idusuario);
        }
        public async Task<Usuario> GetUsuarioId(int idusuario)
        {
            return await this.context.LoadAsync<Usuario>(idusuario);
        }
    }
}
