package ru.neomgtu.proxyrasp.services;

import java.util.List;
import java.util.stream.Collectors;

import org.springframework.stereotype.Service;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;

import lombok.RequiredArgsConstructor;
import reactor.core.publisher.Mono;
import reactor.core.scheduler.Schedulers;
import ru.neomgtu.proxyrasp.dto.SubjectDto;
import ru.neomgtu.proxyrasp.entity.Subject;
import ru.neomgtu.proxyrasp.interfaces.ExternalScheduleServer;
import ru.neomgtu.proxyrasp.repositories.SubjectRepository;

@Service
@RequiredArgsConstructor
public class ScheduleService {

    private final ExternalScheduleServer externalScheduleServer;
    private final SubjectRepository subjectRepository;
    private final ObjectMapper objectMapper;
    
    public Mono<List<SubjectDto>> getScheduleByGroupId(String groupId, String start, String finish, int lng) {
        return externalScheduleServer.getScheduleByGroupId(groupId, start, finish, lng);
    }

    public Mono<List<SubjectDto>> getScheduleByPersonId(String personId, String start, String finish, int lng) {
        return externalScheduleServer.getScheduleByPersonId(personId, start, finish, lng);
    }

    /**
     * Fetch schedule from external server by group id and save into DB.
     */
    public Mono<Void> fetchAndSaveScheduleByGroupId(String groupId, String start, String finish, int lng) {
        return externalScheduleServer.getScheduleByGroupId(groupId, start, finish, lng)
            .flatMap(subjectDtos -> Mono.fromCallable(() -> {
                if (subjectDtos == null || subjectDtos.isEmpty()) {
                    return false;
                }
                List<Subject> entities = subjectDtos.stream()
                    .map(this::toEntity)
                    .collect(Collectors.toList());
                List<Subject> toSave = entities.stream()
                    .filter(s -> !subjectRepository.existsByAuditoriumAndDateAndBeginLesson(s.getAuditorium(), s.getDate(), s.getBeginLesson()))
                    .collect(Collectors.toList());
                if (!toSave.isEmpty()) {
                    subjectRepository.saveAll(toSave);
                }
                return true;
            }).subscribeOn(Schedulers.boundedElastic()))
            .then();
    }

    /**
     * Fetch schedule from external server by person id and save into DB.
     */
    public Mono<Void> fetchAndSaveScheduleByPersonId(String personId, String start, String finish, int lng) {
        return externalScheduleServer.getScheduleByPersonId(personId, start, finish, lng)
            .flatMap(subjectDtos -> Mono.fromCallable(() -> {
                if (subjectDtos == null || subjectDtos.isEmpty()) {
                    return false;
                }
                List<Subject> entities = subjectDtos.stream()
                    .map(this::toEntity)
                    .collect(Collectors.toList());
                List<Subject> toSave = entities.stream()
                    .filter(s -> !subjectRepository.existsByAuditoriumAndDateAndBeginLesson(s.getAuditorium(), s.getDate(), s.getBeginLesson()))
                    .collect(Collectors.toList());
                if (!toSave.isEmpty()) {
                    subjectRepository.saveAll(toSave);
                }
                return true;
            }).subscribeOn(Schedulers.boundedElastic()))
            .then();
    }

    private Subject toEntity(SubjectDto dto) {
        Subject s = new Subject();
        s.setAuditorium(dto.getAuditorium());
        s.setBeginLesson(dto.getBeginLesson());
        s.setBuilding(dto.getBuilding());
        s.setCreateddate(dto.getCreateddate());
        s.setDate(dto.getDate());
        s.setDayOfWeek(dto.getDayOfWeek());
        s.setDayOfWeekString(dto.getDayOfWeekString());
        s.setDiscipline(dto.getDiscipline());
        s.setEndLesson(dto.getEndLesson());
        s.setKindOfWork(dto.getKindOfWork());
        s.setLecturer(dto.getLecturer());
        s.setModifieddate(dto.getModifieddate());
        s.setStream(dto.getStream());
        s.setSubGroup(dto.getSubGroup());
        try {
            if (dto.getListGroups() != null) {
                s.setListGroups(objectMapper.writeValueAsString(dto.getListGroups()));
            }
        } catch (JsonProcessingException e) {
            s.setListGroups(null);
        }
        return s;
    }
}
