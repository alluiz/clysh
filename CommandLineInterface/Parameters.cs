namespace CommandLineInterface
{
    public class Parameters
    {
        public Map<Parameter> Optional { get; private set; }

        public Map<Parameter> Required { get; private set; }

        public Parameters()
        {
            this.Optional = new Map<Parameter>();
            this.Required = new Map<Parameter>();
        }

        public Parameters Add(string name, int minLength, int maxLength, bool required = true)
        {
            Parameter parameter = new Parameter(name, minLength, maxLength, required);

            if (required)
                this.Required.Add(parameter);
            else
                this.Optional.Add(parameter);

            return this;
        }

        public bool Waiting()
        {
            return this.Required.Itens.Count(x => x.Value.Data == null) > 0;
        }
    }
}