$(document).ready(function () {
  const searchInput = $("#searchPhone");
  const clearButton = $("#clearSearch");
  const contractTable = $("#contractTable");
  const noResults = $("#noResults");
  const tuNgayInput = $("#tuNgay");
  const denNgayInput = $("#denNgay");
  const trangThaiFilter = $("#trangThaiFilter");
  const btnFilter = $("#btnFilter");
  const btnReset = $("#btnReset");
  const filterResult = $("#filterResult");
  const filterMessage = $("#filterMessage");
  const ajaxPaginationContainer = $("#ajaxPaginationContainer");
  const originalPaginationContainer = $(".pagination-container");
  let searchTimeout;
  let isFiltering = false;
  let currentPage = 1;

  // Khởi tạo giá trị mặc định cho date inputs
  const today = new Date();
  const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
  tuNgayInput.val(firstDayOfMonth.toISOString().split("T")[0]);
  denNgayInput.val(today.toISOString().split("T")[0]);

  // Xử lý sự kiện lọc theo khoảng thời gian
  btnFilter.on("click", function () {
    performDateFilter();
  });

  // Xử lý sự kiện làm mới
  btnReset.on("click", function () {
    resetFilter();
  });

  // Xử lý sự kiện khi thay đổi ngày
  tuNgayInput.on("change", function () {
    if (tuNgayInput.val() && denNgayInput.val()) {
      currentPage = 1; // Reset về trang 1 khi thay đổi ngày
      performDateFilter();
    }
  });

  denNgayInput.on("change", function () {
    if (tuNgayInput.val() && denNgayInput.val()) {
      currentPage = 1; // Reset về trang 1 khi thay đổi ngày
      performDateFilter();
    }
  });

  // Xử lý sự kiện khi thay đổi trạng thái
  trangThaiFilter.on("change", function () {
    if (tuNgayInput.val() || denNgayInput.val()) {
      currentPage = 1; // Reset về trang 1 khi thay đổi trạng thái
      performDateFilter();
    }
  });

  // Hàm lọc theo khoảng thời gian
  function performDateFilter() {
    if (isFiltering) return;

    const tuNgay = tuNgayInput.val();
    const denNgay = denNgayInput.val();
    const trangThai = trangThaiFilter.val();

    // Kiểm tra validation
    if (tuNgay && denNgay && new Date(tuNgay) > new Date(denNgay)) {
      showFilterMessage(
        "Ngày bắt đầu không thể lớn hơn ngày kết thúc",
        "warning"
      );
      return;
    }

    isFiltering = true;
    btnFilter
      .prop("disabled", true)
      .html(
        '<span class="spinner-border spinner-border-sm"></span> Đang lọc...'
      );

    // Hiển thị loading
    $("#tableLoading").show();
    contractTable.hide();
    $("#noResults").hide();

    $.ajax({
      url: "/QuanLyHopDong/FilterByDateRange",
      type: "GET",
      data: {
        tuNgay: tuNgay,
        denNgay: denNgay,
        trangThai: trangThai,
        page: currentPage,
        pageSize: 10,
      },
      success: function (response) {
        if (response.success) {
          // Cập nhật bảng
          contractTable.find("tbody").html(response.html);

          // Hiển thị thông báo
          showFilterMessage(response.message, "info");

          // Hiển thị bảng hoặc thông báo không có kết quả
          if (response.count > 0) {
            contractTable.show();
            $("#noResults").hide();

            // Ẩn phân trang gốc và hiển thị phân trang AJAX
            originalPaginationContainer.hide();
            if (response.pagination) {
              ajaxPaginationContainer.html(response.pagination).show();
            } else {
              ajaxPaginationContainer.hide();
            }

            // Highlight các dòng nếu có tìm kiếm
            if (searchInput.val()) {
              performSearch(searchInput.val());
            }
          } else {
            contractTable.hide();
            $("#noResults").show();
            originalPaginationContainer.hide();
            ajaxPaginationContainer.hide();
          }
        } else {
          showFilterMessage(response.message, "danger");
          contractTable.hide();
          $("#noResults").show();
          originalPaginationContainer.hide();
          ajaxPaginationContainer.hide();
        }
      },
      error: function (xhr, status, error) {
        showFilterMessage("Có lỗi xảy ra khi lọc dữ liệu: " + error, "danger");
        contractTable.hide();
        $("#noResults").show();
      },
      complete: function () {
        isFiltering = false;
        btnFilter
          .prop("disabled", false)
          .html('<i class="bi bi-funnel"></i> Lọc');
        $("#tableLoading").hide();
      },
    });
  }

  // Hàm làm mới bộ lọc
  function resetFilter() {
    tuNgayInput.val(firstDayOfMonth.toISOString().split("T")[0]);
    denNgayInput.val(today.toISOString().split("T")[0]);
    trangThaiFilter.val("");
    searchInput.val("");
    clearButton.hide();
    currentPage = 1;

    // Ẩn phân trang AJAX và hiển thị phân trang gốc
    ajaxPaginationContainer.hide();
    originalPaginationContainer.show();

    // Reload trang để lấy dữ liệu gốc
    location.reload();
  }

  // Hàm hiển thị thông báo
  function showFilterMessage(message, type) {
    filterMessage.text(message);
    filterResult
      .removeClass("alert-info alert-warning alert-danger")
      .addClass("alert-" + type);
    filterResult.show();

    // Tự động ẩn sau 5 giây
    setTimeout(function () {
      filterResult.fadeOut();
    }, 5000);
  }

  // Hàm set khoảng thời gian nhanh
  function setDateRange(range) {
    const today = new Date();
    let startDate, endDate;

    switch (range) {
      case "today":
        startDate = today;
        endDate = today;
        break;
      case "yesterday":
        startDate = new Date(today.getTime() - 24 * 60 * 60 * 1000);
        endDate = startDate;
        break;
      case "thisWeek":
        const dayOfWeek = today.getDay();
        const diff = today.getDate() - dayOfWeek + (dayOfWeek === 0 ? -6 : 1);
        startDate = new Date(today.setDate(diff));
        endDate = new Date();
        break;
      case "thisMonth":
        startDate = new Date(today.getFullYear(), today.getMonth(), 1);
        endDate = new Date();
        break;
      case "lastMonth":
        startDate = new Date(today.getFullYear(), today.getMonth() - 1, 1);
        endDate = new Date(today.getFullYear(), today.getMonth(), 0);
        break;
      default:
        return;
    }

    tuNgayInput.val(startDate.toISOString().split("T")[0]);
    denNgayInput.val(endDate.toISOString().split("T")[0]);

    // Reset về trang 1 và tự động lọc
    currentPage = 1;
    performDateFilter();
  }

  // Show/hide clear button
  searchInput.on("input", function () {
    const value = $(this).val();
    if (value.length > 0) {
      clearButton.show();
    } else {
      clearButton.hide();
    }

    // Clear previous timeout
    clearTimeout(searchTimeout);

    // Set new timeout for search
    searchTimeout = setTimeout(function () {
      performSearch(value);
    }, 300); // 300ms delay
  });

  // Clear search
  clearButton.on("click", function () {
    searchInput.val("");
    clearButton.hide();
    performSearch("");
    searchInput.focus();
  });

  // Perform search function
  function performSearch(searchValue) {
    const rows = contractTable.find("tbody tr");
    let hasResults = false;

    if (searchValue === "") {
      // Show all rows
      rows.show().removeClass("highlight-row");
      contractTable.show();
      noResults.hide();
      return;
    }

    // Search logic
    rows.each(function () {
      const row = $(this);
      const phone = row.data("phone");

      if (phone && phone.toString().includes(searchValue)) {
        row.show().addClass("highlight-row");
        hasResults = true;
      } else {
        row.hide().removeClass("highlight-row");
      }
    });

    // Show/hide no results message
    if (hasResults) {
      contractTable.show();
      noResults.hide();
    } else {
      contractTable.hide();
      noResults.show();
    }
  }

  // Format phone number input (optional)
  searchInput.on("keypress", function (e) {
    // Only allow numbers, spaces, and common phone characters
    const char = String.fromCharCode(e.which);
    if (!/[0-9\s\-\+\(\)]/.test(char)) {
      e.preventDefault();
    }
  });

  // Handle Enter key
  searchInput.on("keydown", function (e) {
    if (e.key === "Enter") {
      e.preventDefault();
      const foundRows = contractTable.find("tbody tr:visible");
      if (foundRows.length === 1) {
        // If only one result, go to detail
        const detailLink = foundRows.find('a[title="Chi tiết"]');
        if (detailLink.length > 0) {
          window.location.href = detailLink.attr("href");
        }
      }
    }
  });

  // Function tạo hóa đơn từ index
  window.taoHoaDonTuIndex = function (maHopDong, tenKhach) {
    console.log("Tao hoa don tu index:", maHopDong, tenKhach);

    // Set giá trị
    $("#maHopDongIndex").val(maHopDong);
    $("#tenKhachHangIndex").text(tenKhach);
    $("#ghiChuHoaDonIndex").val("");

    // Hiển thị modal
    var myModal = new bootstrap.Modal(
      document.getElementById("taoHoaDonIndexModal")
    );
    myModal.show();
  };

  window.xacNhanTaoHoaDonIndex = function () {
    var maHopDong = $("#maHopDongIndex").val();
    var ghiChu = $("#ghiChuHoaDonIndex").val();

    // Hiển thị loading
    var btn = $("#taoHoaDonIndexModal .modal-footer .btn-success");
    var originalText = btn.html();
    btn
      .html(
        '<span class="spinner-border spinner-border-sm"></span> Đang tạo...'
      )
      .prop("disabled", true);

    $.ajax({
      url: "/QuanLyHoaDon/TaoHoaDon",
      type: "POST",
      data: {
        maHopDong: maHopDong,
        ghiChu: ghiChu,
        __RequestVerificationToken: $(
          'input[name="__RequestVerificationToken"]'
        ).val(),
      },
      success: function (response) {
        if (response.success) {
          if (typeof toastr !== "undefined") {
            toastr.success(response.message);
          } else {
            alert(response.message);
          }
          setTimeout(function () {
            location.reload();
          }, 1000);
        } else {
          if (typeof toastr !== "undefined") {
            toastr.error(response.message);
          } else {
            alert("Lỗi: " + response.message);
          }
          btn.html(originalText).prop("disabled", false);
        }
      },
      error: function (xhr, status, error) {
        var errorMsg = "Có lỗi xảy ra: " + error;
        if (typeof toastr !== "undefined") {
          toastr.error(errorMsg);
        } else {
          alert(errorMsg);
        }
        btn.html(originalText).prop("disabled", false);
      },
      complete: function () {
        bootstrap.Modal.getInstance(
          document.getElementById("taoHoaDonIndexModal")
        ).hide();
      },
    });
  };

  // Function để đóng notification banner
  window.closeNotification = function () {
    const notification = document.getElementById("newOrderNotification");
    if (notification) {
      // Thêm animation fade out
      notification.style.animation = "slideOutUp 0.3s ease-in forwards";

      // Xóa element sau khi animation hoàn thành
      setTimeout(function () {
        notification.remove();
      }, 300);

      // Lưu vào localStorage để không hiện lại trong phiên làm việc
      localStorage.setItem(
        "hiddenNotification_" + new Date().toDateString(),
        "true"
      );
    }
  };

  // Kiểm tra xem có nên hiện notification không (tùy chọn)
  const notification = document.getElementById("newOrderNotification");
  if (notification) {
    const today = new Date().toDateString();
    const isHidden = localStorage.getItem("hiddenNotification_" + today);

    if (isHidden === "true") {
      notification.style.display = "none";
    }
  }

  // Hàm chuyển trang
  window.changePage = function (page) {
    currentPage = page;
    performDateFilter();

    // Scroll to top of table
    $("html, body").animate(
      {
        scrollTop: contractTable.offset().top - 100,
      },
      500
    );
  };

  // Export functions to global scope
  window.setDateRange = setDateRange;
});
