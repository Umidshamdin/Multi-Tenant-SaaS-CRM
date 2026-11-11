namespace SalesFlow.Domain.Enums;

public enum QuoteStatus { Draft = 1, Sent = 2, Accepted = 3, Rejected = 4 }
public enum SalesOrderStatus { Pending = 1, Fulfilled = 2, Cancelled = 3 }
public enum InvoiceStatus { Draft = 1, Issued = 2, Paid = 3, Voided = 4 }
public enum CrmRecordKind { Customer = 1, Lead = 2, Deal = 3, Contact = 4, Product = 5 }
public enum CustomFieldType { Text = 1, Number = 2, Boolean = 3, Date = 4 }
