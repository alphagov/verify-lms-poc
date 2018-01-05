using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace test_website.Pages
{
    public class LMSModel : PageModel
    {
        public string Message { get; set; }

        public void OnGet()
        {
            Message = "The main work horse of this website.";
        }
    }
}
