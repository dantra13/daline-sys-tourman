namespace Sport.Core.DisciplineRegistry;

public enum GenderCode
{
    M,
    W,
    X,
    O,
}

public static class GenderCodeExtensions
{
    public static char ToRscChar(this GenderCode code) => code switch
    {
        GenderCode.M => 'M',
        GenderCode.W => 'W',
        GenderCode.X => 'X',
        GenderCode.O => 'O',
        _ => throw new ArgumentOutOfRangeException(nameof(code)),
    };
}
