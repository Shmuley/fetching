namespace Fetching.Parsers
{
    public static class CliInputParser
    {
        public static string ParseInputArgs(string[] args)
        {
            if (args.Length != 1)
            {
                throw new NotSupportedException("This application requires exactly one argument in the form of a file path.");
            }

            try
            {
                return Path.GetFullPath(args[0]);
            }
            catch (Exception excep)
            {
                throw new FileNotFoundException($"Could not find valid file at path {args[0]}", excep);
            }
        }

    }
}
