from __future__ import annotations

from PySide6.QtCore import QRectF, Qt
from PySide6.QtGui import (
    QColor,
    QFont,
    QPainter,
    QPen,
)
from PySide6.QtWidgets import (
    QLabel,
    QListWidget,
    QMessageBox,
    QPushButton,
    QSpinBox,
    QTableWidget,
    QTableWidgetItem,
    QVBoxLayout,
    QWidget,
)

from round_robin_service import (
    RRProcess,
    RRSegment,
    RoundRobinResult,
    RoundRobinService,
)


class RoundRobinGanttWidget(QWidget):
    """Widget tự vẽ biểu đồ Gantt của Round Robin."""

    def __init__(
        self,
        parent: QWidget | None = None,
    ) -> None:
        super().__init__(parent)

        self.segments: list[RRSegment] = []
        self.visible_count = 0
        self.process_colors: dict[str, QColor] = {}

        self.palette = [
            QColor("#2563eb"),
            QColor("#16a34a"),
            QColor("#ea580c"),
            QColor("#9333ea"),
            QColor("#dc2626"),
            QColor("#0891b2"),
            QColor("#ca8a04"),
            QColor("#db2777"),
            QColor("#4f46e5"),
            QColor("#65a30d"),
        ]

        self.setMinimumHeight(150)

    def set_segments(
        self,
        segments: list[RRSegment],
        visible_count: int | None = None,
    ) -> None:
        self.segments = segments

        if visible_count is None:
            self.visible_count = len(segments)
        else:
            self.visible_count = max(
                0,
                min(
                    visible_count,
                    len(segments),
                ),
            )

        self._assign_colors()
        self.update()

    def clear_chart(self) -> None:
        self.segments = []
        self.visible_count = 0
        self.process_colors.clear()
        self.update()

    def _assign_colors(self) -> None:
        self.process_colors.clear()
        color_index = 0

        for segment in self.segments:
            if segment.pid == "IDLE":
                continue

            if segment.pid not in self.process_colors:
                self.process_colors[segment.pid] = (
                    self.palette[
                        color_index % len(self.palette)
                    ]
                )
                color_index += 1

    def paintEvent(self, event) -> None:
        painter = QPainter(self)

        try:
            painter.setRenderHint(
                QPainter.RenderHint.Antialiasing
            )

            painter.fillRect(
                self.rect(),
                QColor("white"),
            )

            if not self.segments:
                painter.setPen(
                    QColor("#64748b")
                )
                painter.drawText(
                    self.rect(),
                    Qt.AlignmentFlag.AlignCenter,
                    "Chưa có dữ liệu Gantt Chart",
                )
                return

            visible_segments = self.segments[
                : self.visible_count
            ]

            if not visible_segments:
                return

            final_time = max(
                segment.end_time
                for segment in self.segments
            )

            if final_time <= 0:
                return

            left_margin = 35
            right_margin = 25
            top_margin = 30
            chart_height = 55

            available_width = max(
                100,
                self.width()
                - left_margin
                - right_margin,
            )

            time_scale = available_width / final_time

            font = QFont()
            font.setBold(True)
            painter.setFont(font)

            for segment in visible_segments:
                x = (
                    left_margin
                    + segment.start_time
                    * time_scale
                )

                width = max(
                    2,
                    (
                        segment.end_time
                        - segment.start_time
                    )
                    * time_scale,
                )

                rect = QRectF(
                    x,
                    top_margin,
                    width,
                    chart_height,
                )

                if segment.pid == "IDLE":
                    fill_color = QColor("#cbd5e1")
                    text_color = QColor("#334155")
                else:
                    fill_color = self.process_colors.get(
                        segment.pid,
                        QColor("#2563eb"),
                    )
                    text_color = QColor("white")

                painter.fillRect(
                    rect,
                    fill_color,
                )

                painter.setPen(
                    QPen(
                        QColor("#334155"),
                        1,
                    )
                )
                painter.drawRect(rect)

                painter.setPen(text_color)
                painter.drawText(
                    rect,
                    Qt.AlignmentFlag.AlignCenter,
                    segment.pid,
                )

                painter.setPen(
                    QColor("#334155")
                )
                painter.drawText(
                    int(x - 5),
                    int(
                        top_margin
                        + chart_height
                        + 22
                    ),
                    str(segment.start_time),
                )

            last_segment = visible_segments[-1]
            last_x = (
                left_margin
                + last_segment.end_time
                * time_scale
            )

            painter.drawText(
                int(last_x - 5),
                int(
                    top_margin
                    + chart_height
                    + 22
                ),
                str(last_segment.end_time),
            )

        finally:
            painter.end()
class RoundRobinController:
    def __init__(
        self,
        page: QWidget,
    ) -> None:
        self.page = page

        # Hỗ trợ cả objectName theo tài liệu và các tên thay thế.
        self.spin_quantum = self._find_any(
            QSpinBox,
            "spinRRQuantum",
            "spinQuantum",
        )
        self.btn_add = self._find_any(
            QPushButton,
            "btnRRAddProcess",
            "btnAdd",
        )
        self.btn_delete = self._find_any(
            QPushButton,
            "btnRRDeleteProcess",
            "btnDelete",
        )
        self.btn_sample = self._find_any(
            QPushButton,
            "btnRRLoadSample",
            "btnSample",
        )
        self.btn_run = self._find_any(
            QPushButton,
            "btnRRRun",
            "btnRun",
        )
        self.btn_step = self._find_any(
            QPushButton,
            "btnRRStep",
            "btnStep",
        )
        self.btn_reset = self._find_any(
            QPushButton,
            "btnRRReset",
            "btnReset",
        )
        self.input_table = self._find_any(
            QTableWidget,
            "tblRRInput",
            "tblInput",
        )
        self.ready_queue = self._find_any(
            QListWidget,
            "listReadyQueue",
            "listRRReadyQueue",
        )
        self.lbl_current_time = self._find_any(
            QLabel,
            "lblCurrentTime",
            "lblRRCurrentTime",
        )
        self.lbl_running = self._find_any(
            QLabel,
            "lblRunning",
            "lblRRRunningProcess",
        )
        self.lbl_remaining = self._find_any(
            QLabel,
            "lblRemaining",
            "lblRRRemainingTime",
        )
        self.lbl_quantum = self._find_any(
            QLabel,
            "lblQuantum",
            "lblRRCurrentQuantum",
        )
        self.gantt_area = self._find_any(
            QWidget,
            "ganttArea",
            "rrGanttArea",
        )
        self.result_table = self._find_any(
            QTableWidget,
            "tblResult",
            "tblRRResult",
        )
        self.lbl_avg_waiting = self._find_any(
            QLabel,
            "lblAvgWaiting",
            "lblRRAverageWaiting",
        )
        self.lbl_avg_turnaround = self._find_any(
            QLabel,
            "lblAvgTurnaround",
            "lblRRAverageTurnaround",
        )
        self.lbl_avg_response = self._find_any(
            QLabel,
            "lblAvgResponse",
            "lblRRAverageResponse",
        )
        self.lbl_sequence = self._find_any(
            QLabel,
            "lblSequence",
            "lblRRSequence",
        )
        self.lbl_status = self._find_any(
            QLabel,
            "lblStatus",
            "lblRRStatus",
        )

        self.current_result: RoundRobinResult | None = None
        self.current_step = 0
        self.data_changed = True
        self.internal_change = False

        self.gantt_chart = RoundRobinGanttWidget(
            self.gantt_area
        )
        self._insert_gantt_chart()

        self._configure_widgets()
        self._connect_events()

    def _find_any(
        self,
        widget_type: type,
        *names: str,
    ):
        for name in names:
            widget = self.page.findChild(
                widget_type,
                name,
            )
            if widget is not None:
                return widget

        raise RuntimeError(
            "Không tìm thấy objectName nào sau đây "
            "trong RR.ui:\n"
            + "\n".join(names)
        )

    def _insert_gantt_chart(self) -> None:
        layout = self.gantt_area.layout()

        if layout is None:
            layout = QVBoxLayout(
                self.gantt_area
            )

        layout.setContentsMargins(
            0,
            0,
            0,
            0,
        )
        layout.setSpacing(0)
        layout.addWidget(
            self.gantt_chart
        )

    def _configure_widgets(self) -> None:
        self.spin_quantum.setRange(1, 50)
        self.spin_quantum.setValue(3)

        self.input_table.setColumnCount(3)
        self.input_table.setHorizontalHeaderLabels(
            [
                "Process",
                "Arrival Time",
                "Burst Time",
            ]
        )
        self.input_table.setSelectionBehavior(
            QTableWidget.SelectionBehavior.SelectRows
        )
        self.input_table.setSelectionMode(
            QTableWidget.SelectionMode.SingleSelection
        )
        self.input_table.setAlternatingRowColors(True)

        self.result_table.setColumnCount(5)
        self.result_table.setHorizontalHeaderLabels(
            [
                "Process",
                "Completion",
                "Turnaround",
                "Waiting",
                "Response",
            ]
        )
        self.result_table.setEditTriggers(
            QTableWidget.EditTrigger.NoEditTriggers
        )
        self.result_table.setSelectionBehavior(
            QTableWidget.SelectionBehavior.SelectRows
        )
        self.result_table.setAlternatingRowColors(True)

        self.input_table.horizontalHeader().setStretchLastSection(
            True
        )
        self.result_table.horizontalHeader().setStretchLastSection(
            True
        )

        self.lbl_quantum.setText(
            f"Quantum: {self.spin_quantum.value()}"
        )

        self.reset_output()

    def _connect_events(self) -> None:
        self.btn_add.clicked.connect(
            self.add_process
        )
        self.btn_delete.clicked.connect(
            self.delete_process
        )
        self.btn_sample.clicked.connect(
            self.load_sample
        )
        self.btn_run.clicked.connect(
            self.run_all
        )
        self.btn_step.clicked.connect(
            self.run_step
        )
        self.btn_reset.clicked.connect(
            self.reset_all
        )
        self.spin_quantum.valueChanged.connect(
            self.quantum_changed
        )
        self.input_table.cellChanged.connect(
            self.mark_data_changed
        )

    def quantum_changed(
        self,
        value: int,
    ) -> None:
        self.lbl_quantum.setText(
            f"Quantum: {value}"
        )
        self.mark_data_changed()

    def mark_data_changed(self, *args) -> None:
        if self.internal_change:
            return

        self.current_result = None
        self.current_step = 0
        self.data_changed = True

        self.ready_queue.clear()
        self.gantt_chart.clear_chart()
        self.result_table.setRowCount(0)

        self.lbl_status.setText(
            "Trạng thái: Dữ liệu đã thay đổi"
        )

    def add_process(self) -> None:
        self.internal_change = True

        try:
            row = self.input_table.rowCount()
            self.input_table.insertRow(row)

            values = [
                f"P{row + 1}",
                "0",
                "1",
            ]

            for column, value in enumerate(values):
                self.input_table.setItem(
                    row,
                    column,
                    QTableWidgetItem(value),
                )

            self.input_table.setCurrentCell(
                row,
                0,
            )

        finally:
            self.internal_change = False

        self.mark_data_changed()

    def delete_process(self) -> None:
        row = self.input_table.currentRow()

        if row < 0:
            QMessageBox.warning(
                self.page,
                "Chưa chọn Process",
                "Hãy chọn một Process cần xóa.",
            )
            return

        self.input_table.removeRow(row)
        self.mark_data_changed()

    def load_sample(self) -> None:
        sample = [
            ("P1", 0, 10),
            ("P2", 1, 4),
            ("P3", 2, 5),
            ("P4", 3, 3),
        ]

        self.internal_change = True

        try:
            self.input_table.setRowCount(
                len(sample)
            )

            for row, values in enumerate(sample):
                for column, value in enumerate(values):
                    self.input_table.setItem(
                        row,
                        column,
                        QTableWidgetItem(
                            str(value)
                        ),
                    )

            self.spin_quantum.setValue(3)

        finally:
            self.internal_change = False

        self.mark_data_changed()

        self.lbl_status.setText(
            "Trạng thái: Đã nạp dữ liệu mẫu"
        )

    def read_processes(self) -> list[RRProcess]:
        row_count = self.input_table.rowCount()

        if row_count == 0:
            raise ValueError(
                "Danh sách Process đang trống."
            )

        processes: list[RRProcess] = []
        used_pids: set[str] = set()

        for row in range(row_count):
            pid_item = self.input_table.item(
                row,
                0,
            )
            arrival_item = self.input_table.item(
                row,
                1,
            )
            burst_item = self.input_table.item(
                row,
                2,
            )

            if (
                pid_item is None
                or arrival_item is None
                or burst_item is None
            ):
                raise ValueError(
                    f"Dòng {row + 1} chưa nhập đủ dữ liệu."
                )

            pid = pid_item.text().strip()

            if not pid:
                raise ValueError(
                    f"Dòng {row + 1}: "
                    "Tên Process không được để trống."
                )

            if pid in used_pids:
                raise ValueError(
                    f"Process '{pid}' bị trùng tên."
                )

            try:
                arrival = int(
                    arrival_item.text().strip()
                )
                burst = int(
                    burst_item.text().strip()
                )
            except ValueError as error:
                raise ValueError(
                    f"Dòng {row + 1}: "
                    "Arrival Time và Burst Time "
                    "phải là số nguyên."
                ) from error

            if arrival < 0:
                raise ValueError(
                    f"Dòng {row + 1}: "
                    "Arrival Time không được âm."
                )

            if burst <= 0:
                raise ValueError(
                    f"Dòng {row + 1}: "
                    "Burst Time phải lớn hơn 0."
                )

            used_pids.add(pid)

            processes.append(
                RRProcess(
                    pid=pid,
                    arrival_time=arrival,
                    burst_time=burst,
                    order=row,
                )
            )

        return processes

    def calculate_result(self) -> RoundRobinResult:
        return RoundRobinService.run(
            source_processes=self.read_processes(),
            quantum=self.spin_quantum.value(),
        )

    def run_all(self) -> None:
        try:
            self.current_result = (
                self.calculate_result()
            )
            self.current_step = len(
                self.current_result.steps
            )
            self.data_changed = False

            self.display_final_result(
                self.current_result
            )

            self.gantt_chart.set_segments(
                self.current_result.segments
            )

            self.ready_queue.clear()

            self.lbl_current_time.setText(
                "Current Time: "
                f"{self.current_result.segments[-1].end_time}"
            )
            self.lbl_running.setText(
                "Running: Hoàn thành"
            )
            self.lbl_remaining.setText(
                "Remaining: 0"
            )
            self.lbl_status.setText(
                "Trạng thái: Đã chạy xong Round Robin"
            )

        except Exception as error:
            QMessageBox.critical(
                self.page,
                "Lỗi Round Robin",
                str(error),
            )
            self.lbl_status.setText(
                "Trạng thái: Có lỗi"
            )

    def run_step(self) -> None:
        try:
            if (
                self.current_result is None
                or self.data_changed
            ):
                self.current_result = (
                    self.calculate_result()
                )
                self.current_step = 0
                self.data_changed = False
                self.reset_output()

            total_steps = len(
                self.current_result.steps
            )

            if self.current_step >= total_steps:
                self.display_final_result(
                    self.current_result
                )
                QMessageBox.information(
                    self.page,
                    "Hoàn thành",
                    "Round Robin đã chạy hết.",
                )
                return

            step = self.current_result.steps[
                self.current_step
            ]
            self.current_step += 1

            self.ready_queue.clear()
            for pid in step.ready_queue_after:
                self.ready_queue.addItem(pid)

            self.lbl_current_time.setText(
                f"Current Time: {step.end_time}"
            )
            self.lbl_running.setText(
                f"Running: {step.running_pid}"
            )
            self.lbl_remaining.setText(
                "Remaining: "
                f"{step.remaining_after}"
            )
            self.lbl_quantum.setText(
                "Quantum: "
                f"{self.current_result.quantum}"
            )

            visible_segments = (
                self._visible_segment_count(
                    self.current_step
                )
            )

            self.gantt_chart.set_segments(
                self.current_result.segments,
                visible_count=visible_segments,
            )

            partial_sequence = (
                self.current_result.sequence[
                    : self.current_step
                ]
            )

            # SỬA: dùng partial_sequence thay vì result.sequence
            self.lbl_sequence.setText(
                "Sequence: "
                + " → ".join(partial_sequence)
            )

            self.lbl_status.setText(
                "Trạng thái: Bước "
                f"{self.current_step}/{total_steps} - "
                f"{step.running_pid} chạy "
                f"{step.start_time} → {step.end_time}"
            )

            if self.current_step == total_steps:
                self.display_final_result(
                    self.current_result
                )
                self.lbl_status.setText(
                    "Trạng thái: Hoàn thành Round Robin"
                )

        except Exception as error:
            QMessageBox.critical(
                self.page,
                "Lỗi chạy từng bước",
                str(error),
            )

    def _visible_segment_count(
        self,
        completed_steps: int,
    ) -> int:
        """
        Đếm số đoạn Gantt cần hiện.
        Bao gồm cả đoạn IDLE trước những bước đã chạy.
        """
        process_segments_seen = 0

        for index, segment in enumerate(
            self.current_result.segments
        ):
            if segment.pid != "IDLE":
                process_segments_seen += 1

            if process_segments_seen >= completed_steps:
                return index + 1

        return len(self.current_result.segments)

    def display_final_result(
        self,
        result: RoundRobinResult,
    ) -> None:
        self.result_table.setRowCount(
            len(result.processes)
        )

        for row, process in enumerate(
            result.processes
        ):
            values = [
                process.pid,
                process.completion_time,
                process.turnaround_time,
                process.waiting_time,
                process.response_time,
            ]

            for column, value in enumerate(values):
                item = QTableWidgetItem(
                    str(value)
                )
                item.setTextAlignment(
                    Qt.AlignmentFlag.AlignCenter
                )
                self.result_table.setItem(
                    row,
                    column,
                    item,
                )

        self.result_table.resizeColumnsToContents()

        self.lbl_avg_waiting.setText(
            "Average Waiting: "
            f"{result.average_waiting:.2f}"
        )
        self.lbl_avg_turnaround.setText(
            "Average Turnaround: "
            f"{result.average_turnaround:.2f}"
        )
        self.lbl_avg_response.setText(
            "Average Response: "
            f"{result.average_response:.2f}"
        )
        self.lbl_sequence.setText(
            "Sequence: "
            + " → ".join(result.sequence)
        )

    def reset_output(self) -> None:
        self.ready_queue.clear()
        self.result_table.setRowCount(0)
        self.gantt_chart.clear_chart()

        self.lbl_current_time.setText(
            "Current Time: 0"
        )
        self.lbl_running.setText(
            "Running: Không có"
        )
        self.lbl_remaining.setText(
            "Remaining: 0"
        )
        self.lbl_quantum.setText(
            f"Quantum: {self.spin_quantum.value()}"
        )
        self.lbl_avg_waiting.setText(
            "Average Waiting: 0"
        )
        self.lbl_avg_turnaround.setText(
            "Average Turnaround: 0"
        )
        self.lbl_avg_response.setText(
            "Average Response: 0"
        )
        self.lbl_sequence.setText(
            "Sequence:"
        )
        self.lbl_status.setText(
            "Trạng thái: Sẵn sàng"
        )

    def reset_all(self) -> None:
        self.internal_change = True

        try:
            self.input_table.setRowCount(0)
            self.spin_quantum.setValue(3)

        finally:
            self.internal_change = False

        self.current_result = None
        self.current_step = 0
        self.data_changed = True
        self.reset_output()