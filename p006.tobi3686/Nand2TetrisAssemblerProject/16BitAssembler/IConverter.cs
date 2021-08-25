namespace _16BitAssembler
{
    interface IConverter
    {
        string AInstruction(string value);
        string Comp(string comp);
        string Dest(string dest);
        string Jump(string jump);
    }
}
