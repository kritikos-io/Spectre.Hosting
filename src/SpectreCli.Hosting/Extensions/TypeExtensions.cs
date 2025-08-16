namespace Kritikos.SpectreCli.Hosting.Extensions;

internal static class TypeExtensions
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

    var currentBaseType = type.BaseType;
    while (currentBaseType != typeof(object) && currentBaseType is not null)
    {
      yield return currentBaseType;
      currentBaseType = currentBaseType.BaseType;
    }
  }
}
