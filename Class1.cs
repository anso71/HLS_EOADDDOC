using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using Agresso.ServerExtension;
using Agresso.Interface.CommonExtension;
using Agresso.Foundation;
using Agresso.Interface.DocArchive;
using Agresso.Interface.Fundamentals.DocArchive;
using Agresso.Meta;

namespace HLS_EOADDDOC
{
    [ServerProgram("EOADDDOC")]
    public class EoAdddoc : ServerProgramBase
    {
        public override void Run()
        {
            string client = ServerAPI.Current.Parameters["client"];
            string docType = ServerAPI.Current.Parameters["doc_type"];
            string docSystem = ServerAPI.Current.Parameters["doc_system"];


			DataTable dataTable = new DataTable("Dokuments");
			IServerDbAPI api = ServerAPI.Current.DatabaseAPI;
			IStatement sql = CurrentContext.Database.CreateStatement();
			sql.Append("select plo.PersonId, plo.UpdatedBy, Doc.Document, Doc.DocumentName, Doc.DocumentType, Doc.FileName, Doc.ContractName from evryone.dbo.DocumentArchives Doc ");
			sql.Append("join evryone.dbo.Employment emp on Doc.CompanyId = emp.CompanyId and Doc.EmploymentId = emp.EmploymentId ");
			sql.Append("join evryone.dbo.Employee plo  on plo.CompanyId = Doc.CompanyId and emp.EmployeeId = plo.EmployeeId ");
			sql.Append("where Doc.CompanyId = '@client'");	
			sql["client"] = client;
			CurrentContext.Database.Read(sql, dataTable);
			foreach (DataRow row in dataTable.Rows)
            {

				Me.API.WriteLog("person: {0}", row["PersonId"]);
			}

			/*/Start Test
			IDocSystem docSystem2 = (IDocSystem)ObjectFactory.CreateInstance(typeof(IDocSystem));
			LibraryEntity libraryEntity = (LibraryEntity)((IDocLibrary)ObjectFactory.CreateInstance(typeof(IDocLibrary))).GetForClient(client, getClients: true).Get(typeof(LibraryEntity));
			ClientContext client2 = new ClientContext(client);
			DataCarrier empty = docSystem2.GetEmpty(client2);
			DocumentEntity documentEntity = (DocumentEntity)empty.Create(typeof(DocumentEntity));
			documentEntity.DocLibrary = libraryEntity.DocLibrary;
			documentEntity.DocType = docType;
			documentEntity.DocGuid = Guid.NewGuid().ToString();
			documentEntity.DocSystemId = docSystem;
			documentEntity.TotalPages = 1;
			documentEntity.LatestRevision = 1;
			empty.Attach(documentEntity);
			documentEntity.Title = title;
			documentEntity.Description = description;
			DocIndexValueEntity docIndexValueEntity = (DocIndexValueEntity)documentEntity.Create(typeof(DocIndexValueEntity));
			docIndexValueEntity.SequenceNo = 1;
			docIndexValueEntity.IndexValue = client;
			empty.Attach(docIndexValueEntity);
			DocIndexValueEntity docIndexValueEntity2 = (DocIndexValueEntity)documentEntity.Create(typeof(DocIndexValueEntity));
			docIndexValueEntity2.SequenceNo = 2;
			docIndexValueEntity2.IndexValue = invoiceId;
			empty.Attach(docIndexValueEntity2);
			using (MemoryStream fromStream = new MemoryStream(data))
			{
				return docSystem2.CreateNewDocument(client2, docSystem, "1234", empty, fromStream, fileExtension, "");
			}*/
				//End test
		}


	}
}
