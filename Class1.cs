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
			string resource_id = ServerAPI.Current.Parameters["resource_id"];
			string days = ServerAPI.Current.Parameters["days"];

			string gikk = "";

			DataTable dataTable = new DataTable("Dokuments");
			IServerDbAPI api = ServerAPI.Current.DatabaseAPI;
			IStatement sql = CurrentContext.Database.CreateStatement();
			sql.Append("select doc.DocumentArchiveId,emp.AssignmentId,plo.PersonId, plo.PersonName, plo.UpdatedBy, Doc.Pdf, Doc.DocumentName, Doc.DocumentType, Doc.FileName, Doc.ContractName from evryone.dbo.DocumentArchives Doc ");
			sql.Append("join evryone.dbo.Employment emp on Doc.CompanyId = emp.CompanyId and Doc.EmploymentId = emp.EmploymentId ");
			sql.Append("join evryone.dbo.Employee plo  on plo.CompanyId = Doc.CompanyId and emp.EmployeeId = plo.EmployeeId ");
			sql.Append("where Doc.CompanyId = @client  and doc.SignatureStatus is NULL and doc.UpdatedAt > getDate() - @days");	
			sql["client"] = client;
			sql["days"] = Int32.Parse(days);
			CurrentContext.Database.Read(sql, dataTable);
			foreach (DataRow row in dataTable.Rows)
			{
				bool Leggesinn = true;
				DataTable dataTableDoc = new DataTable("ArkDoc");
				IStatement sqldoc = CurrentContext.Database.CreateStatement();
				sqldoc.Append("select doc_index_3 from halostagr_test.dbo.adsindex where doc_type = @doc_type and doc_library = @doc_system and doc_index_3= @documentArchiveId");
				sqldoc["doc_type"] = docType;
				sqldoc["doc_system"] = docSystem;
				sqldoc["documentArchiveId"] = row["DocumentArchiveId"].ToString();
				string doc_index_3 = "";
				if (CurrentContext.Database.ReadValue(sqldoc, ref doc_index_3))
				{
					Leggesinn = false;
				}



				if (Leggesinn)
				{
					Me.API.WriteLog("person: {0}", row["PersonId"]);
					//Start Test
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
					string title = row["AssignmentId"] + row["ContractName"].ToString() + "-" + row["PersonId"].ToString();
					documentEntity.Title = title;
					string description = row["PersonName"].ToString() + ";" + row["PersonId"] + ";" + row["ContractName"].ToString();
					documentEntity.Description = row["PersonName"].ToString();
					DocIndexValueEntity docIndexValueEntity = (DocIndexValueEntity)documentEntity.Create(typeof(DocIndexValueEntity));
					docIndexValueEntity.SequenceNo = 1;
					docIndexValueEntity.IndexValue = client;
					empty.Attach(docIndexValueEntity);
					DocIndexValueEntity docIndexValueEntity2 = (DocIndexValueEntity)documentEntity.Create(typeof(DocIndexValueEntity));
					docIndexValueEntity2.SequenceNo = 2;
					docIndexValueEntity2.IndexValue = resource_id;
					DocIndexValueEntity docIndexValueEntity3 = (DocIndexValueEntity)documentEntity.Create(typeof(DocIndexValueEntity));
					docIndexValueEntity3.SequenceNo = 3;
					docIndexValueEntity3.IndexValue = row["DocumentArchiveId"].ToString();
					empty.Attach(docIndexValueEntity2);
					using (MemoryStream fromStream = new MemoryStream(Byte.Parse(row["Pdf"].ToString())))
					{
						gikk = docSystem2.CreateNewDocument(client2, docSystem, "1234", empty, fromStream, "pdf", "");
					}

					Me.API.WriteLog(" Gikk dette {0}", gikk);
				}
			}
		}


	}
}
