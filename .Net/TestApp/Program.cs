// See https://aka.ms/new-console-template for more information

using Com.Cat.Grpc;
using Google.Protobuf;
using Grpc.Net.Client;
using static Com.Cat.Grpc.Okapi;

Console.WriteLine("Hello, World!");

string ServerAddress = "http://localhost:50051"; // Adjust the address/port as needed
using var channel = GrpcChannel.ForAddress(ServerAddress);
var client = new OkapiClient(channel);
var request = new CreateXliffFromDocumentRequest
{
    FileName = "text.txt",
    FileContent = ByteString.CopyFrom(File.ReadAllBytes("C:\\Alpar\\Test.txt")),
    FilterContent = ByteString.Empty,
    FilterName = "",
    SourceLangISO6391 = "en",
    TargetLangISO6391 = "fr"
};
var response = await client.CreateXliffFromDocumentAsync(request);


int a = 0;
