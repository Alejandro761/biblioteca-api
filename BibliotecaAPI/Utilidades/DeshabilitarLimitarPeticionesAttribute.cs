namespace BibliotecaAPI.Utilidades
{
    // especificamos que este atributo puede usarse en metodos y clases y que no se puede usar mas de una vez en un mismo lugar
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)] 
    public class DeshabilitarLimitarPeticionesAttribute: Attribute
    {
        
    }
}
