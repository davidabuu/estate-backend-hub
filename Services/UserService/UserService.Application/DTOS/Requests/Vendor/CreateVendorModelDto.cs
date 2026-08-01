namespace UserService.Application.DTOS.Requests.Vendor
{
	public record CreateVendorModelDto
    {
       
        public int VendorId { get; set; } 

        public string? Name { get; set; } 

        public string?EstateId { get; set; }

        public string? ServiceType { get; set; } 

      
        public string? AccountNumber { get; set; }

        public string? Duration { get; set; } 
        public string? Amount { get; set; }
        public string? ContactName { get; set; } 

        public string? NoOfWorkers { get; set; }
        public string? Phone { get; set; }

        public DateTime ServiceDate { get; set; }


 

        public bool IsActive { get; set; } = true;


    }
}
