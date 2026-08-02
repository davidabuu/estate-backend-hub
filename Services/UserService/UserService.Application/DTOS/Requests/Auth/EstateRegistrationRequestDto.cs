using UserService.Application.Enums;
using UserService.Domain.Enums;

namespace UserService.Application.DTOS.Requests.Auth
{
	public record EstateRegistrationRequestDto 
    {
 
       
        public string? Email { get; set; }
      
        public string? PhoneNumber { get; set; }
        
        public string? Password { get; set; }
	

		public UserType? RegisterAs { get; set; }
      public string? EstateAddress { get; set; }
		 
		public string? EstateName { get; set; }
        public string? BankName { get; set; }

        public string? BankCode { get; set; }

        public string? AccountName { get; set; }

		public string? AccountNumber { get; set; }
        public List<PropertyType>? PropertyTypes { get; set; } 
  
      
        public bool? IsRegistered { get; set; } = false;

    }
}
