namespace ClusterKit.Core.Rest.ActionMessages
{
    /// <summary>
    /// Standard response from sthe <seealso cref="RestActionMessage{TData,TId}"/> request
    /// </summary>
    /// <typeparam name="TData">The type of entity</typeparam>
    public class RestActionResponse<TData>
    {
        /// <summary>
        /// Gets or sets the object itself.
        /// </summary>
        public TData Data { get; set;}

        /// <summary>
        /// Gets or sets some extra data, that was sent with the request
        /// </summary>
        public object ExtraData { get; set; }

        /// <summary>
        /// Initializes the new instance of <seealso cref="RestActionResponse{TData}"/>
        /// </summary>
        public RestActionResponse()
        {
        }

        /// <summary>
        /// Initializes the new instance of <seealso cref="RestActionResponse{TData}"/>
        /// </summary>
        /// <param name="data">The actual entity data</param>
        /// <param name="extraData">Some extra data, that was sent with the request</param>
        public RestActionResponse(TData data, object extraData)
        {
            this.Data = data;
            this.ExtraData = extraData;
        }
    }
}
