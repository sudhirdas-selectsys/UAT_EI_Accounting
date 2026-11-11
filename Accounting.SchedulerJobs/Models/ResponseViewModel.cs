namespace Accounting.SchedulerJobs.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a generic response view model.
    /// </summary>
    /// <typeparam name="T">Type of the response data.</typeparam>
    public class ResponseViewModel<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseViewModel{T}"/> class.
        /// Default constructor for ResponseViewModel.
        /// </summary>
        public ResponseViewModel()
        {
            this.Error = new List<Errors>();
        }

        /// <summary>
        /// Gets or sets the status code.
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the description of the status.
        /// </summary>
        public string StatusDescription { get; set; }

        /// <summary>
        /// Gets or sets the response data.
        /// </summary>
        public T Response { get; set; }

        /// <summary>
        /// Gets or sets the list of errors.
        /// </summary>
        public List<Errors> Error { get; set; }

        /// <summary>
        /// Standard Error Response.
        /// </summary>
        public class Errors
        {
            /// <summary>
            /// Gets or sets the error code.
            /// </summary>
            public int Code { get; set; }

            /// <summary>
            /// Gets or sets the description of the error.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Gets or sets the error message.
            /// </summary>
            public string Message { get; set; }

            /// <summary>
            /// Gets or sets additional information about the error.
            /// </summary>
            public string More_Info { get; set; }
        }
    }
}
