namespace Generator;
using LLVMSharp;
using static IRGen;

public class Type : Base
{
    AST.Type type;

    public Type(AST.Node node)
    {
        this.type = (AST.Type)node;
    }

    public override void generate()
    {
        // LLVMTypeRef llvmType = getBasicType();
        if (!type.isArray)
        {
            if (type.isPointer)
            {
                genPointer();
                return;
            }
            genNonArray();
        }
        else
        {
            if (type.size == null)
            {
                genPointer();
                return;
            }
            else
            {
                uint count = (uint)type.size;
                typeStack.Push(LLVM.ArrayType(getBasicType(), count));
            }
        }
    }

    public LLVMTypeRef getBasicType()
    {
        if (namedTypesLLVM.ContainsKey(type.value))
        {
            return namedTypesLLVM[type.value];
        }
        (bool isInt, int bits) = Parser.checkInt(type.value);
        if (isInt)
        {
            return LLVM.IntType((uint)bits);
        }
        switch (type.value)
        {
            case "double":
                return LLVM.DoubleType();
            case "string":
                //TODO: implement strings as stdlib so they can have a sane type
                return LLVM.ArrayType(LLVM.Int8Type(), 0);
            case "null":
                return LLVM.VoidType();
            default:
                throw GenException.FactoryMethod("An unknown type was referenced", "Make it a known type, or remove it", this.type, true, this.type.value);
        }
    }

    public void genPointer()
    {
        typeStack.Push(LLVM.PointerType(getBasicType(), 0));
    }

    public void genNonArray()
    {
        typeStack.Push(getBasicType());
    }
}
