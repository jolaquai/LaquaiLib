using System.Dynamic;
using System.Text.RegularExpressions;

namespace LaquaiLib.Util.DynamicExtensions;

public class DynamicMatch : DynamicObject
{
    private readonly Match _match;

    private DynamicMatch(Match match)
    {
        _match = match;
    }
    public static dynamic Create(Match match) => new DynamicMatch(match);

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        if (_match.Groups.TryGetValue(binder.Name, out var group))
        {
            result = group.Value;
            return true;
        }

        result = null;
        return false;
    }
}
