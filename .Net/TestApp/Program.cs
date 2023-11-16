// See https://aka.ms/new-console-template for more information

using Com.Cat.Grpc;
using Google.Protobuf;
using Grpc.Net.Client;
using Proto;
using static Com.Cat.Grpc.Okapi;
using static Proto.CAT;

Console.WriteLine("Hello, World!");

string ServerAddress = "http://192.168.190.230:50051"; // Adjust the address/port as needed
//string ServerAddress = "http://188.166.136.248:50051"; // Adjust the address/port as needed
//string ServerAddress = "http://159.89.249.226:5001"; // Adjust the address/port as needed

using var channel = GrpcChannel.ForAddress("http://localhost:50051");
var client1 = new OkapiClient(channel);
var request1 = new Com.Cat.Grpc.CreateXliffFromDocumentRequest
{
    FileName = "text.txt",
    FileContent = ByteString.CopyFrom(File.ReadAllBytes("C:\\Alpar\\Test.txt")),
    FilterContent = ByteString.Empty,
    FilterName = "",
    SourceLangISO6391 = "en",
    TargetLangISO6391 = "fr"
};
var response = await client1.CreateXliffFromDocumentAsync(request1);


//var channel2 = GrpcChannel.ForAddress(ServerAddress);
//var client2 = new CATClient(channel);

//var request2 = new TMExistsRequest { TmId = "1/_1_en_fr_marketing" };
//var response2 = await client2.TMExistsAsync(request1);


//var channel3 = GrpcChannel.ForAddress("http://143.198.240.169:5001");
var channel3 = GrpcChannel.ForAddress("http://localhost:5001");
//var client3 = new CATClient(channel3);
//var request3 = new Proto.CreateXliffFromDocumentRequest
//{
//    //FileName = "text.txt",
//    //FileContent = ByteString.CopyFrom(File.ReadAllBytes("C:\\Alpar\\Test.txt")),
//    FileName = "Janet Yellen.docx",
//    FileContent = ByteString.CopyFrom(File.ReadAllBytes("C:\\Alpar\\Janet Yellen.docx")),
//    FilterName = "okf_openxml@default-okf_openxml.fprm",
//    FilterContent = ByteString.CopyFrom(File.ReadAllBytes("C:\\Development\\CATLab\\contents\\filters\\okf_openxml@default-okf_openxml.fprm")),
//    SourceLangISO6391 = "en",
//    TargetLangISO6391 = "fr"
//};
//var response3 = await client3.CreateXliffFromDocumentAsync(request3);

var client4 = new CATClient(channel3);
var request4 = new Proto.GetStatisticsForDocumentRequest
{
    //FileName = "text.txt",
    //FileContent = ByteString.CopyFrom(File.ReadAllBytes("C:\\Alpar\\Test.txt")),
    FileName = "Janet Yellen.docx",
    FileContent = ByteString.CopyFrom(File.ReadAllBytes("C:\\Alpar\\Janet Yellen.docx")),
    FilterName = "okf_openxml@default-okf_openxml.fprm",
    FilterContent = ByteString.CopyFrom(File.ReadAllBytes("C:\\Development\\CATLab\\contents\\filters\\okf_openxml@default-okf_openxml.fprm")),
    SourceLangISO6391 = "en",
    TargetLangsISO6391 = { "fr" }
};

var response4 = await client4.GetStatisticsForDocumentAsync(request4);

Console.WriteLine("End.");
