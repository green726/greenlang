using System.Text;
using static System.Text.Json.JsonSerializer;
using System.Linq;

public static class Parser
{
    public static List<ASTNode> nodes = new List<ASTNode>();
    public static List<Util.Token> tokenList;

    public static Util.TokenType[] binaryExpectedTokens = { Util.TokenType.Number };
    public static Util.TokenType[] delimiterExpectedTokens = { Util.TokenType.Keyword };
    public static ASTNode.NodeType[] binaryExpectedNodes = { ASTNode.NodeType.NumberExpression, ASTNode.NodeType.BinaryExpression };

    public static class topAST
    {
        public static List<ASTNode> primaryChildren = new List<ASTNode>();
    }



    public static void checkNode(ASTNode? node, ASTNode.NodeType[] expectedTypes)
    {
        if (node == null)
        {
            throw new ArgumentException($"expected a node at (line and column goes here) but got null");
        }

        foreach (ASTNode.NodeType expectedNodeType in expectedTypes)
        {
            if (node.nodeType != expectedNodeType && expectedNodeType == expectedTypes.Last())
            {
                throw new ArgumentException($"expected type {string.Join(", ", expectedTypes)} but got {node.nodeType}");
            }
            else if (node.nodeType == expectedNodeType)
            {
                break;
            }
        }
    }

    public static void checkToken(Util.Token? token, Util.TokenType[]? expectedTypes = null, Util.TokenType? expectedType = null)
    {
        if (token == null)
        {
            throw new ArgumentException($"expected a token at {token.line}:{token.column} but got null");
        }

        if (expectedTypes != null)
        {
            foreach (Util.TokenType expectedTokenType in expectedTypes)
            {
                if (token.type != expectedTokenType && expectedTokenType == expectedTypes.Last())
                {
                    throw new ArgumentException($"expected token of type {string.Join(", ", expectedTypes)} but got {token.type} at {token.line}:{token.column}");
                }
                else if (token.type == expectedTokenType)
                {
                    break;
                }
            }

        }
        else
        {
            if (token.type != expectedType)
            {
                throw new ArgumentException($"expected token of type {expectedType} but got {token.type} at {token.line}:{token.column}");
            }
        }
    }

    public static string printBinary(BinaryExpression bin)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append($"{bin.nodeType} op: {bin.operatorType} lhs type: {bin.leftHand.nodeType} rhs type: {bin.rightHand.nodeType} binop children below:");
        stringBuilder.Append(printASTRet(bin.children));

        return stringBuilder.ToString();
    }

    public static string printFunc(FunctionAST func)
    {
        StringBuilder stringBuilder = new StringBuilder();
        stringBuilder.Append($"{func.nodeType} name: {func.prototype.name} args: {Serialize(func.prototype.arguments.ToList())} body start: ");

        stringBuilder.Append(printASTRet(func.body));

        stringBuilder.Append("function body end");

        return stringBuilder.ToString();
    }

    public static string printFuncCall(FunctionCall funcCall)
    {
        return $"{funcCall.nodeType} with name of {funcCall.functionName} and args of {String.Join(", ", funcCall.args)}";
    }

    public static string printASTRet(List<ASTNode> nodesPrint)
    {
        StringBuilder stringBuilder = new StringBuilder();

        foreach (ASTNode node in nodesPrint)
        {
            switch (node.nodeType)
            {
                case ASTNode.NodeType.BinaryExpression:
                    BinaryExpression bin = (BinaryExpression)node;
                    stringBuilder.Append(printBinary(bin));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.Function:
                    FunctionAST func = (FunctionAST)node;
                    stringBuilder.Append(printFunc(func));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.FunctionCall:
                    FunctionCall funcCall = (FunctionCall)node;
                    stringBuilder.Append(printFuncCall(funcCall));
                    stringBuilder.Append("\n");
                    break;
                default:
                    stringBuilder.Append(node.nodeType);
                    stringBuilder.Append("\n");
                    break;
            }


        }
        return stringBuilder.ToString();
    }

    public static void printAST(List<ASTNode> nodesPrint)
    {
        StringBuilder stringBuilder = new StringBuilder();
        // Console.WriteLine(nodesPrint[0]);

        foreach (ASTNode node in nodesPrint)
        {
            switch (node.nodeType)
            {
                case ASTNode.NodeType.BinaryExpression:
                    BinaryExpression bin = (BinaryExpression)node;
                    stringBuilder.Append(printBinary(bin));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.Function:
                    FunctionAST func = (FunctionAST)node;
                    stringBuilder.Append(printFunc(func));
                    stringBuilder.Append("\n");
                    break;
                case ASTNode.NodeType.FunctionCall:
                    FunctionCall funcCall = (FunctionCall)node;
                    stringBuilder.Append(printFuncCall(funcCall));
                    stringBuilder.Append("\n");
                    break;
                default:
                    stringBuilder.Append(node.nodeType);
                    stringBuilder.Append("\n");
                    break;
            }


        }

        Console.WriteLine(stringBuilder);
    }


    public static List<Util.Token> getTokensUntil(int startIndex, Util.TokenType stopType)
    {
        List<Util.Token> ret = new List<Util.Token>();
        Util.Token currentToken = tokenList[startIndex];
        int currentIndex = startIndex;

        while (currentToken.type != stopType)
        {
            ret.Add(currentToken);

            currentToken = tokenList[currentIndex + 1];
            currentIndex++;
        }
        return ret;
    }

    public static ASTNode parseKeyword(Util.Token token, int tokenIndex, ASTNode? parent = null)
    {
        List<dynamic> ret = new List<dynamic>();
        Util.Token nextToken = tokenList[tokenIndex + 1];
        int nextTokenIndex = tokenIndex + 1;

        switch (nextToken.type)
        {
            case Util.TokenType.SquareDelimiterOpen:
                break;
            case Util.TokenType.ParenDelimiterOpen:
                //treat it as a function call
                //token would be the name, next token would be delim, so we grab all tokens starting from the one after that until final delim
                FunctionCall funcCall = new FunctionCall(token, null, true);
                return funcCall;
        }
        return null;

    }

    public static bool parseTokenRecursive(Util.Token token, int tokenIndex, ASTNode? parent = null, Util.TokenType[]? expectedTypes = null)
    {

        ASTNode? previousNode = nodes.Count > 0 ? nodes.Last() : null;

        // Console.WriteLine($"parse loop {tokenIndex}: {printAST()}");
        if (token.type == Util.TokenType.EOF)
        {
            return true;
        }

        if (expectedTypes != null)
        {
            checkToken(token, expectedTypes);
        }

        switch (token.type)
        {
            case Util.TokenType.Number:
                new NumberExpression(token, parent);
                break;

            case Util.TokenType.Operator:
                BinaryExpression binExpr = new BinaryExpression(token, previousNode, tokenList[tokenIndex + 1], parent);
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, binExpr, binaryExpectedTokens);

            case Util.TokenType.Keyword:
                ASTNode keyword = parseKeyword(token, tokenIndex, parent);
                //0 is the keyword ASTNode, 1 is the next token, and 2 is the next token index
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, keyword);
        }

        if (token.isDelim)
        {
            if (parent != null)
                return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1, parent);
        }
        return parseTokenRecursive(tokenList[tokenIndex + 1], tokenIndex + 1);

    }


    public static List<ASTNode> beginParse(List<Util.Token> _tokenList)
    {
        tokenList = _tokenList;

        parseTokenRecursive(tokenList[0], 0);

        List<Util.Token> protoArgs = new List<Util.Token>();

        protoArgs.Add(new Util.Token(Util.TokenType.Keyword, "string", 0, 0));
        protoArgs.Add(new Util.Token(Util.TokenType.Keyword, "format", 0, 0));
        protoArgs.Add(new Util.Token(Util.TokenType.Keyword, "double", 0, 0));
        protoArgs.Add(new Util.Token(Util.TokenType.Keyword, "x", 0, 0));

        PrototypeAST printProto = new PrototypeAST(0, 0, "printf", protoArgs);
        nodes.Insert(0, printProto);

        Console.WriteLine("BEGIN OF PARSER DEBUG");
        printAST(nodes);
        Console.WriteLine("END OF PARSER DEBUG");

        return nodes;
    }

}
