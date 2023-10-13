namespace Islands.Building
{
    /// <summary>
    /// Basic interface that contains functionality related to clicking on an object
    /// Currently only used by tiles, but will be expanded to buildings in the future
    /// </summary>
    public interface IClickable
    {
        public void Click();
    }
}