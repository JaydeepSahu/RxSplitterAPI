using AutoMapper;
using DomainLayer.Common;
using DomainLayer.Data;
using Repository_Layer.IRepository;
using Service_Layer.CustomServices;
using Service_Layer.ICustomServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service_Layer.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        public IUserDetailService User { get;private set; }
        public IGroupService Group { get; private set; }
        public IGroupMemberService GroupMember { get; private set; }
        public IExpenseService Expense { get; private set; }
        public IMemberInvitationService  MemberInvitation { get; private set; }
        public ITransactionService Transaction { get; private set; }

        public IReportService Report { get; }

        private RxSplitterContext _context;
        private IMapper _mapper;
        private ISprocRepository _sprocRepo;

        public UnitOfWork(RxSplitterContext context, IMapper mapper, ISprocRepository sprocRepo)
        {
            _context = context;
            _mapper = mapper;
            _sprocRepo = sprocRepo;
            User = new UserDetailService(context);
            Group = new GroupService(context, _sprocRepo);
            GroupMember = new GroupMemberService(context, _sprocRepo);
            Expense = new ExpenseService(context, _mapper,_sprocRepo);
            MemberInvitation = new MemberInvitationService(context, _mapper,_sprocRepo);
            Transaction = new TransactionService(context, _mapper, _sprocRepo);
            Report  = new ReportService(context, _mapper,_sprocRepo);
        }
        public void Save()
        {
            _context.SaveChanges();
        }
    }
}
