namespace CATWeb.Services.MT
{
    public interface IMachineTranslator
    {
        String DetectLanguage(String sText);
        String Translate(String sText, String sFrom, String sTo, Object? mtParams);
        void Translate(List<Translatable> translatables, String sFrom, String sTo, Object? mtParams);
        //int AddMemoryContent(int idMemory, String sTMXContent);
    }
}
