namespace Generator;

using static IRGen;
using LLVMSharp;

public class StructImplement : Base
{
    public AST.StructImplement implement;

    public StructImplement(AST.StructImplement implement)
    {
        this.implement = implement;
    }

    public override void generate()
    {
        // implement.modifyFunctions();

        if (implement.trait.protos.Count > implement.functions.Count)
        {
            throw GenException.FactoryMethod("Not all trait functions implemented", "Implements all trait functions", implement);
        }
        else if (implement.trait.protos.Count < implement.functions.Count)
        {
            throw GenException.FactoryMethod("Too many trait functions implemented", "Implements ONLY trait functions", implement);
        }

        foreach (AST.Function func in implement.functions)
        {
            func.generator.generate();
        }

        base.generate();
    }
}
