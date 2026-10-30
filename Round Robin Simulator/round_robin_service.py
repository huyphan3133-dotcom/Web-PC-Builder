from __future__ import annotations

from collections import deque
from dataclasses import dataclass, field


@dataclass
class RRProcess:
    """Thông tin một tiến trình trong mô phỏng Round Robin."""

    pid: str
    arrival_time: int
    burst_time: int
    order: int = 0
    remaining_time: int = field(init=False)
    first_start_time: int | None = field(
        default=None,
        init=False,
    )
    completion_time: int = field(
        default=0,
        init=False,
    )

    def __post_init__(self) -> None:
        self.remaining_time = self.burst_time

    def clone(self) -> "RRProcess":
        return RRProcess(
            pid=self.pid,
            arrival_time=self.arrival_time,
            burst_time=self.burst_time,
            order=self.order,
        )


@dataclass
class RRSegment:
    """Một đoạn trên biểu đồ Gantt."""

    pid: str
    start_time: int
    end_time: int


@dataclass
class RRStep:
    """
    Một bước chạy của Round Robin.

    Mỗi bước tương ứng với một lần process được cấp CPU,
    tối đa bằng một quantum.
    """

    step_number: int
    running_pid: str
    start_time: int
    end_time: int
    executed_time: int
    remaining_after: int
    ready_queue_before: list[str]
    ready_queue_after: list[str]


@dataclass
class RRProcessResult:
    pid: str
    arrival_time: int
    burst_time: int
    completion_time: int
    turnaround_time: int
    waiting_time: int
    response_time: int


@dataclass
class RoundRobinResult:
    quantum: int
    processes: list[RRProcessResult]
    segments: list[RRSegment]
    steps: list[RRStep]
    sequence: list[str]
    average_waiting: float
    average_turnaround: float
    average_response: float


class RoundRobinService:
    """Thực hiện thuật toán Round Robin có Arrival Time."""

    @staticmethod
    def run(
        source_processes: list[RRProcess],
        quantum: int,
    ) -> RoundRobinResult:
        if quantum <= 0:
            raise ValueError(
                "Quantum phải lớn hơn 0."
            )

        if not source_processes:
            raise ValueError(
                "Danh sách Process đang trống."
            )

        processes = [
            process.clone()
            for process in source_processes
        ]

        ordered = sorted(
            processes,
            key=lambda item: (
                item.arrival_time,
                item.order,
            ),
        )

        ready_queue: deque[RRProcess] = deque()
        current_time = 0
        next_index = 0
        completed_count = 0
        segments: list[RRSegment] = []
        steps: list[RRStep] = []
        sequence: list[str] = []

        def add_arrived_processes() -> None:
            """
            Đưa tất cả process đã đến vào cuối Ready Queue.
            """
            nonlocal next_index

            while (
                next_index < len(ordered)
                and ordered[next_index].arrival_time
                <= current_time
            ):
                ready_queue.append(
                    ordered[next_index]
                )
                next_index += 1

        while completed_count < len(processes):
            add_arrived_processes()

            # CPU rảnh vì chưa có process nào đến.
            if not ready_queue:
                if next_index >= len(ordered):
                    break

                next_arrival = (
                    ordered[next_index].arrival_time
                )

                RoundRobinService._append_segment(
                    segments=segments,
                    pid="IDLE",
                    start=current_time,
                    end=next_arrival,
                )

                current_time = next_arrival
                add_arrived_processes()
                continue

            queue_before = [
                process.pid
                for process in ready_queue
            ]

            running = ready_queue.popleft()

            if running.first_start_time is None:
                running.first_start_time = current_time

            start_time = current_time

            executed_time = min(
                quantum,
                running.remaining_time,
            )

            current_time += executed_time
            running.remaining_time -= executed_time

            RoundRobinService._append_segment(
                segments=segments,
                pid=running.pid,
                start=start_time,
                end=current_time,
            )

            sequence.append(running.pid)

            # Process đến trong lúc CPU đang chạy
            # phải vào hàng đợi trước process vừa chạy xong quantum.
            add_arrived_processes()

            if running.remaining_time > 0:
                ready_queue.append(running)
            else:
                running.completion_time = current_time
                completed_count += 1

            queue_after = [
                process.pid
                for process in ready_queue
            ]

            steps.append(
                RRStep(
                    step_number=len(steps) + 1,
                    running_pid=running.pid,
                    start_time=start_time,
                    end_time=current_time,
                    executed_time=executed_time,
                    remaining_after=running.remaining_time,
                    ready_queue_before=queue_before,
                    ready_queue_after=queue_after,
                )
            )

        process_results: list[RRProcessResult] = []

        for process in sorted(
            processes,
            key=lambda item: item.order,
        ):
            turnaround = (
                process.completion_time
                - process.arrival_time
            )

            waiting = (
                turnaround
                - process.burst_time
            )

            response = (
                0
                if process.first_start_time is None
                else process.first_start_time
                - process.arrival_time
            )

            process_results.append(
                RRProcessResult(
                    pid=process.pid,
                    arrival_time=process.arrival_time,
                    burst_time=process.burst_time,
                    completion_time=process.completion_time,
                    turnaround_time=turnaround,
                    waiting_time=max(0, waiting),
                    response_time=max(0, response),
                )
            )

        count = len(process_results)

        average_waiting = (
            sum(
                item.waiting_time
                for item in process_results
            )
            / count
        )

        average_turnaround = (
            sum(
                item.turnaround_time
                for item in process_results
            )
            / count
        )

        average_response = (
            sum(
                item.response_time
                for item in process_results
            )
            / count
        )

        return RoundRobinResult(
            quantum=quantum,
            processes=process_results,
            segments=segments,
            steps=steps,
            sequence=sequence,
            average_waiting=average_waiting,
            average_turnaround=average_turnaround,
            average_response=average_response,
        )

    @staticmethod
    def _append_segment(
        segments: list[RRSegment],
        pid: str,
        start: int,
        end: int,
    ) -> None:
        if end <= start:
            return

        # Giữ nguyên từng lượt Round Robin.
        # Không gộp hai lượt liên tiếp để Sequence dễ quan sát.
        segments.append(
            RRSegment(
                pid=pid,
                start_time=start,
                end_time=end,
            )
        )