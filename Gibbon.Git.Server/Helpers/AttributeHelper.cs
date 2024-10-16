namespace Gibbon.Git.Server.Helpers;

public static class AttributeHelper
{
    public static Dictionary<string, object> Attributes(params string[] attributes)
    {
        var attributeDictionary = new Dictionary<string, object>();

        foreach (var attribute in attributes)
        {
            attributeDictionary[attribute] = attribute;
        }

        return attributeDictionary;
    }
}
