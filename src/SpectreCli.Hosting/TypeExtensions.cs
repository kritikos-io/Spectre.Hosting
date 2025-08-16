namespace Kritikos.SpectreCli.Hosting;

public static class TypeExtensions
{
  public static IEnumerable<Type> GetParentTypes(this Type? type)
  {
    if (type is null)
    {
      yield break;
    }

    foreach (var i in type.GetInterfaces())
    {
      yield return i;
    }

    var baseType = type.BaseType;
    while (baseType != typeof(object) && baseType is not null)
    {
      yield return baseType;
      baseType = baseType.BaseType;
    }
  }
}
