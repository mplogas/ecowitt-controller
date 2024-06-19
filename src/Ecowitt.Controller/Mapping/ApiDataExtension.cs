using Ecowitt.Controller.Model;

namespace Ecowitt.Controller.Mapping;

public static class ApiDataExtension
{
    public static Gateway Map(this ApiData apiData, bool isMetric = true)
    {
        return new Gateway();
    }
}