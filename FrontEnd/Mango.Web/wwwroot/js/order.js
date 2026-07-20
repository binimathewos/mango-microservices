$(document).ready(function () {
  const orderTable = $("#tblOrder");

  orderTable.DataTable({
    order: [0, "desc"],
    ajax: {
      url: "/order/getall",
      type: "GET",
      dataSrc: function (json) {
        console.log("AJAX Response:", json);
        return json.data;
      },
    },
    columns: [
      { data: "orderHeaderId", width: "10%" },
      { data: "email", width: "20%" },
      { data: "name", width: "20%" },
      { data: "phone", width: "20%" },
      { data: "orderStatus", width: "10%" },
      {
        data: "orderTotal",
        width: "10%",
        render: function (data) {
          return "$" + parseFloat(data).toFixed(2);
        },
      },
      {
        data: "orderHeaderId",
        width: "10%",
        render: function (data) {
          return `<div class="w-75 btn-group" role="group">
            <a href="/order/orderdetails?orderId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i></a>
          </div>`;
        },
      },
    ],
  });
});
