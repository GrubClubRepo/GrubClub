using PayPalMvc.Enums;
namespace PayPalMvc {
	/// <summary>
	/// Response received from a transaction registration
	/// </summary>
	public abstract class CommonResponse {
		
        /// <summary>
		/// Acknowledgement of transaction
		/// </summary>
		public ResponseType ACK { get; set; }

        /// <summary>
        /// PayPal Transaction Id
        /// </summary>
        public string CORRELATIONID { get; set; }

        public string TIMESTAMP { get; set; }

        public string VERSION { get; set; }

        public string BUILD { get; set; }


        // For readability
        public ResponseType ResponseStatus { get { return ACK; } } // Stored
        public string RequestId { get { return CORRELATIONID; } } // Stored

        // For error capturing
        public string L_ERRORCODE0 { get; set; }
        public string L_SHORTMESSAGE0 { get; set; }
        public string L_LONGMESSAGE0 { get; set; }
        public string L_SEVERITYCODE0 { get; set; }

        public string ErrorToString // Stored
        {
            get
            {
                if (L_ERRORCODE0 != null || L_SHORTMESSAGE0 != null || L_LONGMESSAGE0 != null || L_SEVERITYCODE0 != null)
                    return string.Format("Error Code: {0} Severity: {1} Message: {2} ({3})", L_ERRORCODE0, L_SEVERITYCODE0, L_SHORTMESSAGE0, L_LONGMESSAGE0);
                else
                    return null;
            }
        }
	}
}