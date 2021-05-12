using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MvcCoreAWSDynamoDb.Services
{
    public class ServiceAWSS3
    {
        private String bucketName;
        private IAmazonS3 awsClient;

        public ServiceAWSS3(IAmazonS3 awsclient, 
            IConfiguration configuration)
        {
            this.awsClient = awsclient;
            this.bucketName = configuration["AWSS3:BucketName"];
        }

        //VAMOS A COMENZAR SUBIENDO FICHEROS AL BUCKET
        //CADA BUCKET TENDRA SUS DATOS (Stream) Y UNA KEY (fileName)
        //UNA VEZ QUE SUBIMOS EL BUCKET, SI ES CORRECTO NOS DEVUELVE
        //UNA RESPUESTA Http 200
        public async Task<bool> UploadFileAsync(Stream stream
            , String fileName)
        {
            //PARA PODER SUBIRLO, SE REALIZA MEDIANTE UNA PETICION
            //REQUEST
            PutObjectRequest request = new PutObjectRequest()
            {
                 InputStream = stream,
                 Key = fileName,
                 BucketName = this.bucketName
            };
            //A TRAVES DEL CLIENTE, REALIZAMOS UNA PETICION CON REQUEST
            //Y NOS DEVUELVE RESPONSE
            PutObjectResponse response =
                await this.awsClient.PutObjectAsync(request);
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<List<String>> GetS3FilesAsync()
        {
            ListVersionsResponse response =
                await this.awsClient.ListVersionsAsync(this.bucketName);
            return response.Versions.Select(x => x.Key).ToList();
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            DeleteObjectResponse response =
                await this.awsClient.DeleteObjectAsync
                (this.bucketName, fileName);
            //AUTOMATICAMENTE, SI ELIMINAMOS DEVUELVE UNA RESPUESTA
            //CORRECTA Y SIN EL CONTENIDO
            if (response.HttpStatusCode == HttpStatusCode.NoContent)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Stream> GetFileAsync(String fileName)
        {
            GetObjectResponse response =
                await this.awsClient.GetObjectAsync(this.bucketName, fileName);
            if (response.HttpStatusCode == HttpStatusCode.OK)
            {
                return response.ResponseStream;
            }
            else
            {
                return null;
            }
        }
    }
}
