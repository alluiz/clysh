namespace CommandLineInterface
{
    public class Arguments
    {
        public Map<Argument> Optional { get; private set; }

        public Map<Argument> Required { get; private set; }

        public Arguments()
        {
            this.Optional = new Map<Argument>();
            this.Required = new Map<Argument>();
        }

        public Arguments Add(string name, int minLength, int maxLength, bool required = true)
        {
            Argument argument = new Argument(name, minLength, maxLength, required);

            if (required)
                this.Required.Add(argument);
            else
                this.Optional.Add(argument);

            return this;
        }

        public bool Waiting()
        {
            return this.Required.Itens.Count(x => x.Value.Data == null) > 0;
        }
    }
}