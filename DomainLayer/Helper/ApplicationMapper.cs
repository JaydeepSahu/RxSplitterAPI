using AutoMapper;
using DomainLayer.Data;
using DomainLayer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainLayer.Helper
{
    public class ApplicationMapper : Profile
    {
        public ApplicationMapper()
        {
            CreateMap<UserDetailDTO, UserDetail>().ReverseMap();
            CreateMap<ExpensesDTO, Expense>().ReverseMap();
            CreateMap<ExpenseTransactionDTO, ExpenseTransaction>().ReverseMap();
            CreateMap<ExpensesDTO, SP_GetAllExpensesDetailsAccGroupId>().ReverseMap();

        }
    }
}